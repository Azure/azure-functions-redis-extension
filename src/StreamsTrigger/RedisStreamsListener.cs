using System;
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
        internal bool deleteAfterProcess;

        public RedisStreamsListener(string connectionString, int pollingInterval, string keys, int count, string consumerGroup, string consumerName, bool deleteAfterProcess, ITriggeredFunctionExecutor executor)
            : base(connectionString, pollingInterval, keys, count, executor)
        {
            this.consumerGroup = consumerGroup;
            this.consumerName = consumerName;
            this.deleteAfterProcess = deleteAfterProcess;
            this.positions = this.keys.Select((key) => new StreamPosition(key, 0)).ToArray();
        }

        public override async Task<bool> PollAsync(CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(consumerGroup) && String.IsNullOrEmpty(consumerName))
            {
                return (await PollNoGroup(cancellationToken)) == 0;
            }

            if (!String.IsNullOrEmpty(consumerGroup) && !String.IsNullOrEmpty(consumerName))
            {
                return (await PollGroup(cancellationToken)) == 0;
            }
            return true;
        }

        private async Task<int> PollNoGroup(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadAsync(positions, count);

            for (int i = 0; i < positions.Length; i++)
            {
                if (streams[i].Entries.Length > 0)
                {
                    positions[i] = new StreamPosition(positions[i].Key, streams[i].Entries.Last().Id);

                    var triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.List,
                        Trigger = streams[i].Key,
                        Message = streams[i].Entries.Select(entry => JsonSerializer.Serialize(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()))).ToArray()
                    };

                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                    if (deleteAfterProcess)
                    {
                        await db.StreamDeleteAsync(positions[i].Key, streams[i].Entries.Select(entry => entry.Id).ToArray());
                    }
                }
            }
            return streams.Sum(stream => stream.Entries.Length);
        }

        private async Task<int> PollGroup(CancellationToken cancellationToken)
        {
            IDatabase db = multiplexer.GetDatabase();
            RedisStream[] streams = await db.StreamReadGroupAsync(positions, consumerGroup, consumerName, count);

            for (int i = 0; i < positions.Length; i++)
            {
                if (streams[i].Entries.Length > 0)
                {
                    positions[i] = new StreamPosition(positions[i].Key, streams[i].Entries.Last().Id);
                    var triggerValue = new RedisMessageModel
                    {
                        TriggerType = RedisTriggerType.List,
                        Trigger = streams[i].Key,
                        Message = streams[i].Entries.Select(entry => JsonSerializer.Serialize(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()))).ToArray()
                    };

                    await executor.TryExecuteAsync(new TriggeredFunctionData() { TriggerValue = triggerValue }, cancellationToken);
                    if (deleteAfterProcess)
                    {
                        await db.StreamDeleteAsync(positions[i].Key, streams[i].Entries.Select(entry => entry.Id).ToArray());
                    }
                }
            }
            return streams.Sum(stream => stream.Entries.Length);
        }
    }
}