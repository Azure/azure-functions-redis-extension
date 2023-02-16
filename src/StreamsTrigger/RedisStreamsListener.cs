using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


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

            // create consumer group for each stream key
            foreach (RedisKey key in keys)
            {
                try
                {
                    logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] Attempting to create consumer group '{consumerGroup}'.");
                    if (await db.StreamCreateConsumerGroupAsync(key, consumerGroup))
                    {
                        logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] Successfully created consumer group '{consumerGroup}'.");
                    }
                    else
                    {
                        logger?.LogCritical($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] Could not create consumer group.");
                        throw new Exception($"Could not create consumer group '{consumerGroup}' for stream key '{key}'.");
                    }
                }
                catch (RedisServerException e)
                {
                    if (e.Message.Contains("BUSYGROUP"))
                    {
                        logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] Consumer group '{consumerGroup}' has already been created.");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadGroupAsync(positions, consumerGroup, consumerName, batchSize);

            for (int i = 0; i < streams.Length; i++)
            {
                if (streams[i].Entries.Length > 0)
                {
                    logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{streams[i].Key}] Received {streams[i].Entries.Length} elements.");
                    foreach (StreamEntry entry in streams[i].Entries)
                    {
                        var triggerValue = new RedisMessageModel
                        {
                            Trigger = streams[i].Key,
                            Message = JsonSerializer.Serialize(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()))
                        };

                        await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                    };

                    RedisValue[] entryIds = streams[i].Entries.Select(entry => entry.Id).ToArray();
                    logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{streams[i].Key}] Acknolwedging entries {entryIds}.");
                    await db.StreamAcknowledgeAsync(streams[i].Key, consumerGroup, entryIds);

                    if (deleteAfterProcess)
                    {
                        await db.StreamDeleteAsync(streams[i].Key, entryIds);
                    }
                }
            };
        }

        public override async void BeforeClosing()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] Deleting consumer group '{consumerGroup}'.");
                long remainingMessages = await db.StreamDeleteConsumerAsync(key, consumerGroup, consumerName);
                logger?.LogInformation($"[{nameof(RedisStreamsListener)}][Consumer:{consumerName}][Key:{key}] There were {remainingMessages} remaing messages for the consumer.");
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
