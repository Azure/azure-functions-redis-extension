﻿using Microsoft.Azure.WebJobs.Host.Executors;
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
    internal sealed class RedisStreamListener : RedisPollingTriggerBaseListener
    {
        internal string consumerName;
        internal string entriesReadKey;

        public RedisStreamListener(string name, IConnectionMultiplexer multiplexer, string key, TimeSpan pollingInterval, int maxBatchSize, bool batch, ITriggeredFunctionExecutor executor, ILogger logger)
            : base(name, multiplexer, key, pollingInterval, maxBatchSize, batch, executor, logger)
        {
            this.consumerName = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID") ?? Guid.NewGuid().ToString();
            this.logPrefix = $"[Name:{name}][Trigger:RedisStreamTrigger][ConsumerGroup:{this.name}][Key:{key}][Consumer:{consumerName}]";
            this.scaleMonitor = new RedisStreamTriggerScaleMonitor(multiplexer, name, maxBatchSize, key);
            this.entriesReadKey = RedisScalerProvider.GetFunctionScalerId(name, "RedisStreamTrigger", key);
        }

        public override async void BeforePolling()
        {
            IDatabase db = multiplexer.GetDatabase();
            try
            {
                logger?.LogInformation($"{logPrefix} Attempting to create consumer group '{name}' for the stream at key '{key}'.");
                if (!await db.StreamCreateConsumerGroupAsync(key, name, StreamPosition.Beginning))
                {
                    logger?.LogCritical($"{logPrefix} Could not create consumer group '{name}' for the stream at key '{key}'.");
                    throw new Exception($"Could not create consumer group '{name}' for the stream at key '{key}'.");
                }
                await db.KeyDeleteAsync(entriesReadKey);
                logger?.LogInformation($"{logPrefix} Successfully created consumer group '{name}' for the stream at key '{key}'.");

            }
            catch (RedisServerException e)
            {
                if (e.Message.Contains("BUSYGROUP"))
                {
                    logger?.LogInformation($"{logPrefix} Consumer group '{name}' for the stream at key '{key}' already exists.");
                }
                else
                {
                    logger?.LogCritical($"{logPrefix} Could not create consumer group '{name}' for the stream at key '{key}'.");
                    throw;
                }
            }

            logger?.LogInformation($"{logPrefix} Beginning polling loop.");
        }

        public override async Task PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            StreamEntry[] entries = await db.StreamReadGroupAsync(key, name, consumerName, count: maxBatchSize);
            logger?.LogDebug($"{logPrefix} Received {entries.Length} entries from the stream at key '{key}'.");
            if (entries.Length == 0)
            {
                return;
            }

            if (batch)
            {
                await ExecuteBatchAsync(entries, cancellationToken);
            }
            else
            {
                await Task.WhenAll(entries.Select(entry => ExecuteAsync(entry, cancellationToken)));
            }
        }

        private async Task ExecuteAsync(StreamEntry value, CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = value }, cancellationToken);

            long acknowledged = await db.StreamAcknowledgeAsync(key, name, value.Id);

            // Redis 6/6.2 has no way to estimate number of entries read by the group, whereas Redis 7 contains entries_read and lag fields in the XINFO GROUPS command
            // This is necessary for the scale controller to accurately estimate the number of function instances needed to process unacked entries.
            if (serverVersion < RedisUtilities.Version70)
            {
                await db.StringIncrementAsync(entriesReadKey);
            }

            logger?.LogDebug($"{logPrefix} Acknowledged {acknowledged} entries from the stream at key '{key}'.");
        }

        private async Task ExecuteBatchAsync(StreamEntry[] values, CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = values }, cancellationToken);

            long acknowledged = await db.StreamAcknowledgeAsync(key, name, Array.ConvertAll(values, value => value.Id));

            // Redis 6/6.2 has no way to estimate number of entries read by the group, whereas Redis 7 contains entries_read and lag fields in the XINFO GROUPS command
            // This is necessary for the scale controller to accurately estimate the number of function instances needed to process unacked entries.
            if (serverVersion < RedisUtilities.Version70)
            {
                await db.StringIncrementAsync(entriesReadKey);
            }

            logger?.LogDebug($"{logPrefix} Acknowledged {acknowledged} entries from the stream at key '{key}'.");
        }

        public async override void BeforeClosing()
        {
            IDatabase db = multiplexer.GetDatabase();
            logger?.LogInformation($"{logPrefix} Attempting to delete consumer name '{consumerName}' from the consumer group '{name}' for the stream at key '{key}'.");
            long pending = await db.StreamDeleteConsumerAsync(key, name, consumerName);
            logger?.LogInformation($"{logPrefix} Successfully deleted consumer name '{consumerName}' from the consumer group '{name}' for the stream at key '{key}'. There were {pending} pending messages for the consumer.");
        }
    }
}
