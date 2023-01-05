using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisListsListener : RedisPollingListenerBase
    {
        internal bool listPopFromBeginning;

        public RedisListsListener(string connectionString, int pollingInterval, int messagesPerWorker, string keys, int count, bool listPopFromBeginning, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, messagesPerWorker, keys, count, executor)
        {
            this.listPopFromBeginning = listPopFromBeginning;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            if (version < new Version("7.0") && keys.Length > 1)
            {
                // lmpop is 7.0 and higher, so function will only be able to trigger on a single key
                throw new ArgumentException($"The cache's {version} is lower than 7.0, and does not support lmpop");
            }

            if (version < new Version("6.2"))
            {
                // count option only introduced in 6.2 and higher
                // changing values here to ensure proper scaling logic
                count = 1;
                messagesPerWorker = 10;
            }
        }

        public override async Task<bool> PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            bool triggered = false;
            if (version >= new Version("7.0"))
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys, count) : await db.ListRightPopAsync(keys, count);
                triggered = result.Values.Length > 0;
                foreach (RedisValue value in result.Values)
                {
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.List,
                        Trigger = result.Key,
                        Message = value
                    };
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }
            }
            else if (version >= new Version("6.2"))
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0], count) : await db.ListRightPopAsync(keys[0], count);
                triggered = result.Length > 0;
                foreach (RedisValue value in result)
                {
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.List,
                        Trigger = keys[0],
                        Message = value
                    };
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }
            }
            else
            {
                var result = listPopFromBeginning ? await db.ListLeftPopAsync(keys[0]) : await db.ListRightPopAsync(keys[0]);
                if (!result.IsNullOrEmpty)
                {
                    triggered = true;
                    RedisMessageModel triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.List,
                        Trigger = keys[0],
                        Message = result
                    };
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }
            }
            return !triggered;
        }

        public override Task<RedisPollingMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingMetrics
            {
                Remaining = keys.Sum((key) => multiplexer.GetDatabase().ListLength(key)),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}
