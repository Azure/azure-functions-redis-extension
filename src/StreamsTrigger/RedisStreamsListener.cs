using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisStreamsListener : RedisPollingTriggerBaseListener
    {
        internal bool deleteAfterProcess;
        internal string consumerGroup;
        internal StreamPosition[] positions;
        internal string consumerName;

        public RedisStreamsListener(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, string consumerGroup, bool deleteAfterProcess, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, executor, logger)
        {
            this.consumerGroup = consumerGroup;
            this.deleteAfterProcess = deleteAfterProcess;
            this.positions = this.keys.Select((key) => new StreamPosition(key, StreamPosition.NewMessages)).ToArray();
            this.consumerName = Guid.NewGuid().ToString();
        }

        public override async void BeforePolling()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                try
                {
                    logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Attempting to create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                    if (!await db.StreamCreateConsumerGroupAsync(key, consumerGroup))
                    {
                        logger?.LogCritical($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                        throw new Exception($"Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                    }
                    logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Successfully created consumer group '{consumerGroup}' for the stream at key '{key}'.");

                }
                catch (RedisServerException e)
                {
                    if (e.Message.Contains("BUSYGROUP"))
                    {
                        logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Consumer group '{consumerGroup}' for the stream at key '{key}' already exists.");
                    }
                    else
                    {
                        logger?.LogCritical($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                        throw;
                    }
                }
            }

            logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Beginning polling loop.");
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadGroupAsync(positions, consumerGroup, consumerName, batchSize);

            foreach (RedisStream stream in streams)
            {
                logger?.LogDebug($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Received {stream.Entries.Length} elements from the stream at key '{stream.Key}'.");
                foreach (StreamEntry entry in stream.Entries)
                {
                    var triggerValue = new RedisMessageModel
                    {
                        Trigger = stream.Key,
                        Message = JsonSerializer.Serialize(entry.Values)
                    };

                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }

                RedisValue[] entryIds = stream.Entries.Select(entry => entry.Id).ToArray();
                long acknowledged = await db.StreamAcknowledgeAsync(stream.Key, consumerGroup, entryIds);
                logger?.LogDebug($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Acknowledged {acknowledged} elements from the stream at key '{stream.Key}'.");

                if (deleteAfterProcess)
                {
                    long deleted = await db.StreamDeleteAsync(stream.Key, entryIds);
                    logger?.LogDebug($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Deleted {deleted} elements from the stream at key '{stream.Key}'.");
                }
            }
        }

        public async override void BeforeClosing()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Attempting to delete consumer name '{consumerName}' from the consumer group '{consumerGroup}' for the stream at key '{key}'.");
                long pending = await db.StreamDeleteConsumerAsync(key, consumerGroup, consumerName);
                logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}] Successfully deleted consumer name '{consumerName}' from the consumer group '{consumerGroup}' for the stream at key '{key}'. There were {pending} pending messages for the consumer.");
            }
        }

        public override Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = keys.Sum((key) => multiplexer.GetDatabase().StreamLength(key)),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}
