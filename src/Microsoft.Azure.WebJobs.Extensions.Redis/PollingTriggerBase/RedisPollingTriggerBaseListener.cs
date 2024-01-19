﻿using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Configuration;
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
        internal IConfiguration configuration;
        internal string connectionStringSetting;
        internal string key;
        internal TimeSpan pollingInterval;
        internal int maxBatchSize;
        internal bool batch;
        internal ITriggeredFunctionExecutor executor;
        internal ILogger logger;

        internal IConnectionMultiplexer multiplexer;
        internal string logPrefix;
        internal Version serverVersion;
        internal RedisPollingTriggerBaseScaleMonitor scaleMonitor;

        public RedisPollingTriggerBaseListener(string name, IConfiguration configuration, string connectionStringSetting, string key, TimeSpan pollingInterval, int maxBatchSize, bool batch, ITriggeredFunctionExecutor executor, ILogger logger)
        {
            this.name = name;
            this.configuration = configuration;
            this.connectionStringSetting = connectionStringSetting;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.maxBatchSize = maxBatchSize;
            this.batch = batch;
            this.executor = executor;
            this.logger = logger;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, connectionStringSetting, name);
            logger?.LogInformation($"{logPrefix} Connecting to Redis.");
            serverVersion = multiplexer.GetServers()[0].Version;
            BeforePolling();
            _ = Task.Run(() => Loop(cancellationToken));
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
