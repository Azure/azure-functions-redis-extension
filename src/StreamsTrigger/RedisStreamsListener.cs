using System;
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
        internal StreamPosition[] positions;
        internal string consumerGroup;
        internal string consumerName;

        public RedisStreamsListener(string connectionString, int pollingInterval, int messagesPerWorker, string keys, int count, string consumerGroup, string consumerName, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, messagesPerWorker, keys, count, executor)
        {
            this.consumerGroup = consumerGroup;
            this.consumerName = consumerName;
            this.positions = this.keys.Select((key) => new StreamPosition(key, 0)).ToArray();
        }

        public override async Task<bool> PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = String.IsNullOrEmpty(consumerGroup) 
                ? await db.StreamReadAsync(positions, count) 
                : await db.StreamReadGroupAsync(positions, consumerGroup, consumerName, count);

            for (int i = 0; i < streams.Length; i++)
            {
                foreach (StreamEntry entry in streams[i].Entries)
                {
                    var triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.Streams,
                        Trigger = streams[i].Key,
                        Message = JsonSerializer.Serialize(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()))
                    };

                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                }
                await db.StreamDeleteAsync(streams[i].Key, streams[i].Entries.Select(entry => entry.Id).ToArray());
            }
            return streams.Sum(stream => stream.Entries.Length) == 0;
        }

        public override Task<RedisPollingMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingMetrics
            {
                Count = keys.Sum((key) => multiplexer.GetDatabase().StreamLength(key)),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}