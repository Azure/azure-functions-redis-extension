using Microsoft.Azure.WebJobs.Host.Executors;
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
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisListListener : RedisPollingTriggerBaseListener
    {
        internal bool listPopFromBeginning;

        public RedisListListener(string id, string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, bool listPopFromBeginning, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, executor, logger)
        {
            this.listPopFromBeginning = listPopFromBeginning;
            this.logPrefix = $"[RedisListTrigger][Id:{id}][Keys:{keys}]";
            this.Descriptor = new ScaleMonitorDescriptor(id, $"{id}-RedisListTrigger");
            this.TargetScalerDescriptor = new TargetScalerDescriptor($"{id}-RedisListTrigger");
        }

        public override void BeforePolling()
        {
            if (serverVersion < new Version("7.0") && keys.Length > 1)
            {
                logger?.LogWarning($"{logPrefix} The cache's version ({serverVersion}) is lower than 7.0 and does not support lmpop. Defaulting to lpop/rpop on the first key given.");
            }

            if (serverVersion < new Version("6.2") && batchSize > 1)
            {
                logger?.LogWarning($"{logPrefix} The cache's version ({serverVersion}) is lower than 6.2 and does not support the COUNT argument in lpop/rpop. Defaulting to lpop/rpop without the COUNT argument, which pulls a single element from the list at a time.");
            }
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            if (serverVersion >= new Version("7.0"))
            {
                ListPopResult result = listPopFromBeginning ? await db.ListLeftPopAsync(keys, batchSize) : await db.ListRightPopAsync(keys, batchSize);
                logger?.LogDebug($"{logPrefix} Received {result.Values.Count()} elements from the list at key '{result.Key}'.");
                foreach (RedisValue value in result.Values)
                {
                    RedisListEntry triggerValue = new RedisListEntry(result.Key, value);
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                };
            }
            else if (serverVersion >= new Version("6.2"))
            {
                RedisValue[] result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0], batchSize) : await db.ListRightPopAsync(keys[0], batchSize);
                logger?.LogDebug($"{logPrefix} Received {result.Length} elements from the list at key '{keys[0]}'.");
                foreach (RedisValue value in result)
                {
                    RedisListEntry triggerValue = new RedisListEntry(keys[0], value);
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                };
            }
            else
            {
                RedisValue result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0]) : await db.ListRightPopAsync(keys[0]);
                logger?.LogDebug($"{logPrefix} Received 1 element from the list at key '{keys[0]}'.");
                if (!result.IsNullOrEmpty)
                {
                    RedisListEntry triggerValue = new RedisListEntry(keys[0], result);
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }
            }
        }

        public override Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = keys.Sum((key) => multiplexer.GetDatabase().ListLength(key)),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}
