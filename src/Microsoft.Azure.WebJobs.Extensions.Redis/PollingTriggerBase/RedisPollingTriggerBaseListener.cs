using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for polling a cache.
    /// </summary>
    internal abstract class RedisPollingTriggerBaseListener : IListener, IScaleMonitorProvider, ITargetScalerProvider
    {
        internal string name;
        internal string connectionString;
        internal string key;
        internal TimeSpan pollingInterval;
        internal int maxBatchSize;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;

        internal string logPrefix;
        internal IConnectionMultiplexer multiplexer;
        internal Version serverVersion;
        internal RedisPollingTriggerBaseScaleMonitor scaleMonitor;

        public RedisPollingTriggerBaseListener(string name, string connectionString, string key, TimeSpan pollingInterval, int maxBatchSize, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.name = name;
            this.connectionString = connectionString;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.maxBatchSize = maxBatchSize;
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
        /// Any Redis commands necessary to run before the connection is terminated.
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

        public IScaleMonitor GetMonitor()
        {
            return scaleMonitor;
        }

        public ITargetScaler GetTargetScaler()
        {
            return scaleMonitor;
        }
    }
}
