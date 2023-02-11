﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Responsible for managing connections and listening to a given Azure Redis Cache.
    /// </summary>
    internal sealed class RedisStreamsListener : RedisPollingListenerBase
    {
        internal bool deleteAfterProcess;
        internal string consumerGroup;
        internal StreamPosition[] positions;
        internal string consumerName;

        public RedisStreamsListener(string connectionString, int pollingInterval, int messagesPerWorker, string keys, int batchSize, string consumerGroup, bool deleteAfterProcess, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, messagesPerWorker, keys, batchSize, executor)
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
            foreach(RedisKey key in keys)
            {
                try
                {
                    if (!await db.StreamCreateConsumerGroupAsync(key, consumerGroup))
                    {
                        throw new Exception($"Could not create consumer group for stream key {key}");
                    }
                }
                catch (RedisServerException e)
                {
                    // consumer group already exists
                    if(!e.Message.Contains("BUSYGROUP"))
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
                    await db.StreamAcknowledgeAsync(streams[i].Key, consumerGroup, entryIds);
                    
                    if (deleteAfterProcess)
                    {
                        await db.StreamDeleteAsync(streams[i].Key, entryIds);
                    }
                }
            };
        }

        public override void BeforeClosing()
        {
            IDatabase db = multiplexer.GetDatabase();
            foreach (RedisKey key in keys)
            {
                db.StreamDeleteConsumerAsync(key, consumerGroup, consumerName);
            }
        }

        public override Task<RedisPollingMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingMetrics
            {
                Remaining = keys.Sum((key) => multiplexer.GetDatabase().StreamLength(key)),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}