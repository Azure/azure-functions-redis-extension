using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Redis instance.
    /// </summary>
    internal sealed class RedisListsListener : RedisPollingTriggerBaseListener
    {
        internal bool listPopFromBeginning;

        public RedisListsListener(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, bool listPopFromBeginning, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, executor, logger)
        {
            this.listPopFromBeginning = listPopFromBeginning;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            if (serverVersion < new Version("7.0") && keys.Length > 1)
            {
                logger?.LogError($"[{nameof(RedisListsListener)}] The Redis version {serverVersion} is lower than 7.0, and does not support LMPOP. Listening to only the first key '{keys[0]}'.");
            }

            if (serverVersion < new Version("6.2") && batchSize > 1)
            {
                logger?.LogError($"[{nameof(RedisListsListener)}] The Redis version {serverVersion} is lower than 6.2, and does not support the COUNT argument in LPOP. Reverting to a batchSize of 1.");
            }
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            if (serverVersion >= new Version("7.0"))
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys, batchSize) : await db.ListRightPopAsync(keys, batchSize);
                logger?.LogInformation($"[{nameof(RedisListsListener)}] Received {result.Values.Count()} elements from the lists.");
                foreach(RedisValue value in result.Values)
                {
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        Trigger = result.Key,
                        Message = value
                    };
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                };
            }
            else if (serverVersion >= new Version("6.2"))
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0], batchSize) : await db.ListRightPopAsync(keys[0], batchSize);
                logger?.LogInformation($"[{nameof(RedisListsListener)}] Received {result.Length} elements from the list at key '{keys[0]}'.");
                foreach (RedisValue value in result)
                {
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        Trigger = keys[0],
                        Message = value
                    };
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                };
            }
            else
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0]) : await db.ListRightPopAsync(keys[0]);
                if (!result.IsNullOrEmpty)
                {
                    logger?.LogInformation($"[{nameof(RedisListsListener)}] Received 1 element from the list at key '{keys[0]}'.");
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        Trigger = keys[0],
                        Message = result
                    };
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
