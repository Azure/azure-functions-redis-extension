﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Scale;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for polling a cache.
    /// </summary>
    internal abstract class RedisPollingListenerBase : IListener, IScaleMonitor, IScaleMonitor<RedisPollingMetrics>
    {
        private const int MINIMUM_SAMPLES = 5;
        internal string connectionString;
        internal RedisKey[] keys;
        internal TimeSpan pollingInterval;
        internal int messagesPerWorker;
        internal int batchSize;
        internal ITriggeredFunctionExecutor executor;
        internal IConnectionMultiplexer multiplexer;
        internal Version serverVersion;

        public ScaleMonitorDescriptor Descriptor => throw new NotImplementedException();

        public RedisPollingListenerBase(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, ITriggeredFunctionExecutor executor)
        {
            this.connectionString = connectionString;
            this.keys = keys.Split(' ').Select(key => new RedisKey(key)).ToArray();
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.batchSize = batchSize;
            this.executor = executor;
        }

        /// <summary>
        /// Executes enabled functions, primary listener method.
        /// </summary>
        public virtual async Task StartAsync(CancellationToken cancellationToken)
        {
            if (multiplexer is null)
            {
                multiplexer = await ConnectionMultiplexer.ConnectAsync(connectionString);
            }

            if (!multiplexer.IsConnected)
            {
                throw new ArgumentException("Failed to connect to cache.");
            }

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
            try
            {
                BeforeClosing();
                await existingMultiplexer.CloseAsync();
                await existingMultiplexer.DisposeAsync();
            }
            catch (Exception)
            {
                throw new Exception("Failed to close connection to cache.");
            }
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

        public abstract Task<RedisPollingMetrics> GetMetricsAsync();

        public ScaleStatus GetScaleStatus(ScaleStatusContext<RedisPollingMetrics> context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.ToArray());
        }

        public ScaleStatus GetScaleStatus(ScaleStatusContext context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.Cast<RedisPollingMetrics>().ToArray());
        }

        private ScaleStatus GetScaleStatusCore(int workerCount, RedisPollingMetrics[] metrics)
        {
            // don't scale up or down if we don't have enough metrics
            if (metrics is null || metrics.Length < MINIMUM_SAMPLES)
            {
                return new ScaleStatus { Vote = ScaleVote.None };
            }

            double average = metrics.Skip(metrics.Length - MINIMUM_SAMPLES).Select(metric => metric.Remaining).Average();

            if (workerCount * messagesPerWorker < average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleOut };
            }

            if ((workerCount - 1) * messagesPerWorker > average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleIn };
            }

            return new ScaleStatus { Vote = ScaleVote.None };
        }
    }
}