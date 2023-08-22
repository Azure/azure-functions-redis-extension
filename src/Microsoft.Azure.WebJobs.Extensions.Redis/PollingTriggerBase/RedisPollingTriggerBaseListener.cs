using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for polling a cache.
    /// </summary>
    internal abstract class RedisPollingTriggerBaseListener : IListener, IScaleMonitor, IScaleMonitor<RedisPollingTriggerBaseMetrics>, ITargetScaler
    {
        private const int MINIMUM_SAMPLES = 5;
        internal string name;
        internal string connectionString;
        internal string key;
        internal TimeSpan pollingInterval;
        internal int count;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;

        internal string logPrefix;
        public ScaleMonitorDescriptor Descriptor { get; internal set; }
        public TargetScalerDescriptor TargetScalerDescriptor { get; internal set; }

        internal IConnectionMultiplexer multiplexer;
        internal Version serverVersion;

        public RedisPollingTriggerBaseListener(string name, string connectionString, string key, TimeSpan pollingInterval, int count, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.name = name;
            this.connectionString = connectionString;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.count = count;
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
                logger?.LogInformation($"{logPrefix} Connecting to Redis.");
                multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
                serverVersion = multiplexer.GetServers()[0].Version;
                BeforePolling();
                _ = Task.Run(() => Loop(cancellationToken));
            }
        }

        /// <summary>
        /// Triggers disconnect from cache when cancellation token is invoked.
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
        /// Closes redis cache multiplexer connection.
        /// </summary>
        internal async Task CloseMultiplexerAsync(IConnectionMultiplexer existingMultiplexer)
        {
            BeforeClosing();
            logger?.LogInformation($"{logPrefix} Closing and disposing multiplexer.");
            await existingMultiplexer.CloseAsync();
            await existingMultiplexer.DisposeAsync();
        }

        /// <summary>
        /// Any Redis commands necessary to run after the connection is created but before the polling starts.
        /// </summary>
        public virtual void BeforePolling() { }

        /// <summary>
        /// Implementation of the logic used to poll the cache.
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
                await PollAsync(cancellationToken);
                await Task.Delay(pollingInterval);
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

            double average = metrics.OrderByDescending(metric => metric.Timestamp).Take(MINIMUM_SAMPLES).Select(metric => metric.Remaining).Average();

            if (workerCount * count < average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleOut };
            }

            if ((workerCount - 1) * count > average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleIn };
            }

            return new ScaleStatus { Vote = ScaleVote.None };
        }

        public async Task<TargetScalerResult> GetScaleResultAsync(TargetScalerContext context)
        {
            RedisPollingTriggerBaseMetrics metric = await GetMetricsAsync();
            return new TargetScalerResult() { TargetWorkerCount = (int)Math.Ceiling(metric.Remaining / (decimal)count) };
        }
    }
}
