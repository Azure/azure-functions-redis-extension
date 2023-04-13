using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
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
    internal sealed class RedisStreamListener : RedisPollingTriggerBaseListener
    {
        internal bool deleteAfterProcess;
        internal string consumerGroup;
        internal StreamPosition[] positions;

        public RedisStreamsListener(string id, string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int batchSize, string consumerGroup, bool deleteAfterProcess, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, executor, logger)
        {
            this.consumerGroup = consumerGroup;
            this.deleteAfterProcess = deleteAfterProcess;
            this.positions = this.keys.Select((key) => new StreamPosition(key, StreamPosition.NewMessages)).ToArray();
            this.logPrefix = $"[RedisStreamTrigger][ConsumerGroup:{consumerGroup}][Consumer:{id}][Keys:{keys}]";
            this.Descriptor = new ScaleMonitorDescriptor(id, $"{id}-RedisStreamTrigger");
            this.TargetScalerDescriptor = new TargetScalerDescriptor($"{id}-RedisStreamTrigger");
        }

        public override async void BeforePolling()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                try
                {
                    logger?.LogInformation($"{logPrefix} Attempting to create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                    if (!await db.StreamCreateConsumerGroupAsync(key, consumerGroup))
                    {
                        logger?.LogCritical($"{logPrefix} Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                        throw new Exception($"Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                    }
                    logger?.LogInformation($"{logPrefix} Successfully created consumer group '{consumerGroup}' for the stream at key '{key}'.");

                }
                catch (RedisServerException e)
                {
                    if (e.Message.Contains("BUSYGROUP"))
                    {
                        logger?.LogInformation($"{logPrefix} Consumer group '{consumerGroup}' for the stream at key '{key}' already exists.");
                    }
                    else
                    {
                        logger?.LogCritical($"{logPrefix} Could not create consumer group '{consumerGroup}' for the stream at key '{key}'.");
                        throw;
                    }
                }
            }

            logger?.LogInformation($"{logPrefix} Beginning polling loop.");
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadGroupAsync(positions, consumerGroup, id, batchSize);

            foreach (RedisStream stream in streams)
            {
                logger?.LogDebug($"{logPrefix} Received {stream.Entries.Length} elements from the stream at key '{stream.Key}'.");
                foreach (StreamEntry entry in stream.Entries)
                {
                    RedisStreamEntry triggerValue = new RedisStreamEntry(stream.Key, entry.Id, entry.Values.Select(a => new KeyValuePair<string, string>(a.Name.ToString(), a.Value.ToString())).ToArray());
                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }

                RedisValue[] entryIds = stream.Entries.Select(entry => entry.Id).ToArray();
                long acknowledged = await db.StreamAcknowledgeAsync(stream.Key, consumerGroup, entryIds);
                logger?.LogDebug($"{logPrefix} Acknowledged {acknowledged} elements from the stream at key '{stream.Key}'.");

                if (deleteAfterProcess)
                {
                    long deleted = await db.StreamDeleteAsync(stream.Key, entryIds);
                    logger?.LogDebug($"{logPrefix} Deleted {deleted} elements from the stream at key '{stream.Key}'.");
                }
            }
        }

        public async override void BeforeClosing()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                logger?.LogInformation($"{logPrefix} Attempting to delete consumer name '{id}' from the consumer group '{consumerGroup}' for the stream at key '{key}'.");
                long pending = await db.StreamDeleteConsumerAsync(key, consumerGroup, id);
                logger?.LogInformation($"{logPrefix} Successfully deleted consumer name '{id}' from the consumer group '{consumerGroup}' for the stream at key '{key}'. There were {pending} pending messages for the consumer.");
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
