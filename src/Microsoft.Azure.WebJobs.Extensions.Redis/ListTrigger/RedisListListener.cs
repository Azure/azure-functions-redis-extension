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

        public RedisListListener(string name, string connectionString, string key, TimeSpan pollingInterval, int maxBatchSize, bool listPopFromBeginning, bool batch, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(name, connectionString, key, pollingInterval, maxBatchSize, batch, executor, logger)
        {
            this.listPopFromBeginning = listPopFromBeginning;
            this.logPrefix = $"[Name:{name}][Trigger:RedisListTrigger][Key:{key}]";
            this.Descriptor = new ScaleMonitorDescriptor(name, $"{name}-RedisListTrigger-{key}");
            this.TargetScalerDescriptor = new TargetScalerDescriptor($"{name}-RedisListTrigger-{key}");
        }

        public override void BeforePolling()
        {
            if (serverVersion < RedisUtilities.Version62 && maxBatchSize > 1)
            {
                logger?.LogWarning($"{logPrefix} The cache's version ({serverVersion}) is lower than 6.2 and does not support the COUNT argument in lpop/rpop. Defaulting to lpop/rpop without the COUNT argument, which pulls a single entry from the list at a time.");
            }
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            if (serverVersion >= RedisUtilities.Version62)
            {
                RedisValue[] result = listPopFromBeginning ? await db.ListLeftPopAsync(key, maxBatchSize) : await db.ListRightPopAsync(key, maxBatchSize);
                if (result is null)
                {
                    logger?.LogDebug($"Key '{key}' does not exist.");
                }
                else
                {
                    logger?.LogDebug($"{logPrefix} Received {result.Length} entries from the list at key '{key}'.");
                    if (batch)
                    {
                        await ExecuteAsync(result, cancellationToken);
                    }
                    else
                    {
                        await Task.WhenAll(result.Select(value => ExecuteAsync(value, cancellationToken)));
                    }
                }
            }
            else
            {
                RedisValue value = listPopFromBeginning ? await db.ListLeftPopAsync(key) : await db.ListRightPopAsync(key);
                if (value.IsNullOrEmpty)
                {
                    logger?.LogDebug($"Key '{key}' does not exist.");
                }
                else
                {
                    logger?.LogDebug($"{logPrefix} Received 1 entry from the list at key '{key}'.");
                    await ExecuteAsync(value, cancellationToken);
                }
            }
        }

        private async Task ExecuteAsync(object value, CancellationToken cancellationToken)
        {
            FunctionResult result = await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = value }, cancellationToken);
            if (!result.Succeeded)
            {
                logger?.LogCritical(result.Exception, "Function execution failed with exception.");
            }
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
