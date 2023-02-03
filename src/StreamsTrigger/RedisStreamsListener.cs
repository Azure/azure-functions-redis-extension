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
        internal bool deleteAfterProcess;

        public RedisStreamsListener(string connectionString, int pollingInterval, int messagesPerWorker, string keys, int batchSize, bool deleteAfterProcess, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, messagesPerWorker, keys, batchSize, executor)
        {
            this.deleteAfterProcess = deleteAfterProcess;
            this.positions = this.keys.Select((key) => new StreamPosition(key, 0)).ToArray();
        }

        public override async Task<bool> PollAsync(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadAsync(positions, batchSize);

            for (int i = 0; i < streams.Length; i++)
            {
                if (streams[i].Entries.Length > 0)
                {
                    foreach (StreamEntry entry in streams[i].Entries)
                    {
                        var triggerValue = new RedisMessageModel
                        {
                            TriggerType = RedisTriggerType.Stream,
                            Trigger = streams[i].Key,
                            Message = JsonSerializer.Serialize(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()))
                        };

                        await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                    };
                    
                    if (deleteAfterProcess)
                    {
                        await db.StreamDeleteAsync(streams[i].Key, streams[i].Entries.Select(entry => entry.Id).ToArray());
                    }
                    else
                    {
                        positions[i] = new StreamPosition(streams[i].Key, streams[i].Entries.Last().Id);
                    }
                }
            };
            return streams.Sum(stream => stream.Entries.Length) == batchSize;
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