﻿using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisListListener : RedisPollingTriggerBaseListener
    {
        internal bool listPopFromBeginning;

        public RedisListListener(string name, string connectionString, string key, TimeSpan pollingInterval, int messagesPerWorker, int count, bool listPopFromBeginning, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(name, connectionString, key, pollingInterval, messagesPerWorker, count, executor, logger)
        {
            this.listPopFromBeginning = listPopFromBeginning;
            this.logPrefix = $"[Name:{name}][Trigger:RedisListTrigger][Key:{key}]";
            this.Descriptor = new ScaleMonitorDescriptor(name, $"{name}-RedisListTrigger-{key}");
            this.TargetScalerDescriptor = new TargetScalerDescriptor($"{name}-RedisListTrigger-{key}");
        }

        public override void BeforePolling()
        {
            if (serverVersion < RedisUtilities.Version62 && count > 1)
            {
                logger?.LogWarning($"{logPrefix} The cache's version ({serverVersion}) is lower than 6.2 and does not support the COUNT argument in lpop/rpop. Defaulting to lpop/rpop without the COUNT argument, which pulls a single element from the list at a time.");
            }
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            if (serverVersion >= RedisUtilities.Version62)
            {
                RedisValue[] result = listPopFromBeginning ? await db.ListLeftPopAsync(key, count) : await db.ListRightPopAsync(key, count);
                logger?.LogDebug($"{logPrefix} Received {result.Length} elements from the list at key '{key}'.");
                await Task.WhenAll(result.Select(value => ExecuteAsync(value, cancellationToken)));
            }
            else
            {
                RedisValue result = listPopFromBeginning ? await db.ListLeftPopAsync(key) : await db.ListRightPopAsync(key);
                logger?.LogDebug($"{logPrefix} Received 1 element from the list at key '{key}'.");
                if (!result.IsNullOrEmpty)
                {
                    await ExecuteAsync(result, cancellationToken);
                }
            }
        }

        private Task ExecuteAsync(RedisValue value, CancellationToken cancellationToken)
        {
            RedisListEntry triggerValue = new RedisListEntry(key, value);
            return executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
        }

        public override Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = multiplexer.GetDatabase().ListLength(key),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}
