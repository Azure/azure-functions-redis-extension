using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for polling a Redis instance.
    /// </summary>
    internal abstract class RedisPollingTriggerBaseListener : IListener, IScaleMonitor, IScaleMonitor<RedisPollingTriggerBaseMetrics>
    {
        private const int MINIMUM_SAMPLES = 5;

        internal string connectionString;
        internal RedisKey[] keys;
        internal TimeSpan pollingInterval;
        internal int messagesPerWorker;
        internal int batchSize;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;

        internal IConnectionMultiplexer multiplexer;
        internal Version serverVersion;

        public ScaleMonitorDescriptor Descriptor => throw new NotImplementedException();

        public RedisPollingTriggerBaseListener(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.connectionString = connectionString;
            this.keys = keys.Split(' ').Select(key => new RedisKey(key)).ToArray();
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.batchSize = batchSize;
            this.executor = executor;
            this.logger = logger;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            if (multiplexer is null)
            {
                logger?.LogInformation("Connecting to Redis.");
                multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }

            if (!multiplexer.IsConnected)
            {
                logger?.LogCritical("Failed to connect to Redis.");
                throw new ArgumentException("Failed to connect to Redis.");
            }

            serverVersion = multiplexer.GetServers()[0].Version;
            BeforePolling();

            logger?.LogInformation($"Beginning main polling loop.");
            _ = Task.Run(() => Loop(cancellationToken));
        }

        /// <summary>
        /// Triggers disconnect from Redis when cancellation token is invoked.
        /// </summary>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await CloseMultiplexerAsync(multiplexer);
        }

        public async void Cancel()
        {
            await CloseMultiplexerAsync(multiplexer);
        }

        public async void Dispose()
        {
            await CloseMultiplexerAsync(multiplexer);
        }


        /// <summary>
        /// Closes redis multiplexer connection.
        /// </summary>
        internal async Task CloseMultiplexerAsync(IConnectionMultiplexer existingMultiplexer)
        {
            logger?.LogInformation("Closing and disposing multiplexer.");
            await existingMultiplexer.CloseAsync();
            await existingMultiplexer.DisposeAsync();
        }

        /// <summary>
        /// Any Redis commands necessary to run after the connection is created but before the polling starts.
        /// </summary>
        public virtual void BeforePolling() { }

        /// <summary>
        /// Implementation of the logic used to poll Redis.
        /// </summary>
        public abstract Task PollAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Any Redis commands necessart to run before the connection is terminated.
        /// </summary>
        public virtual void BeforeClosing() { }

        /// <summary>
        /// Main loop thread.
        /// </summary>
        private async Task Loop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await PollAsync(cancellationToken);
                    await Task.Delay(pollingInterval);
                }
                catch (Exception e)
                {
                    logger?.LogCritical(e, "Critical exception while polling Redis.");
                }
            }
        }

        async Task<ScaleMetrics> IScaleMonitor.GetMetricsAsync()
        {
            return await this.GetMetricsAsync().ConfigureAwait(false);
        }

        public abstract Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync();

        public ScaleStatus GetScaleStatus(ScaleStatusContext<RedisPollingTriggerBaseMetrics> context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.ToArray());
        }

        public ScaleStatus GetScaleStatus(ScaleStatusContext context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.Cast<RedisPollingTriggerBaseMetrics>().ToArray());
        }

        private ScaleStatus GetScaleStatusCore(int workerCount, RedisPollingTriggerBaseMetrics[] metrics)
        {
            // don't scale up or down if we don't have enough metrics
            if (metrics is null || metrics.Length < MINIMUM_SAMPLES)
            {
                return new ScaleStatus { Vote = ScaleVote.None };
            }

            double average = metrics.Skip(metrics.Length - MINIMUM_SAMPLES).Select(metric => metric.Remaining).Average();

            // if we don't have enough capacity, scale up
            if (workerCount * messagesPerWorker < average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleOut };
            }

            // if we can remove one worker and still be above capacity, scale down
            if ((workerCount - 1) * messagesPerWorker > average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleIn };
            }

            return new ScaleStatus { Vote = ScaleVote.None };
        }
    }
}
