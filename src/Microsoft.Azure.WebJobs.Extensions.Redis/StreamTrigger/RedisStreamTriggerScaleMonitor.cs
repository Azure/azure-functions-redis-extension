using Microsoft.Azure.WebJobs.Host.Scale;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisStreamTriggerScaleMonitor : RedisPollingTriggerBaseScaleMonitor
    {
        public RedisStreamTriggerScaleMonitor(IConnectionMultiplexer multiplexer,
            string name,
            int maxBatchSize,
            string key) 
            : base(name, multiplexer, maxBatchSize, key)
        {
            this.Descriptor = new ScaleMonitorDescriptor(name, RedisScalerProvider.GetFunctionScalerId(name, "RedisStreamTrigger", key));
            this.TargetScalerDescriptor = new TargetScalerDescriptor(RedisScalerProvider.GetFunctionScalerId(name, "RedisStreamTrigger", key));
        }

        public override async Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            long streamLength = await multiplexer.GetDatabase().StreamLengthAsync(key);
            if (streamLength == 0)
            {
                return new RedisPollingTriggerBaseMetrics
                {
                    Remaining = 0,
                    Timestamp = DateTime.UtcNow,
                };
            }

            StreamGroupInfo[] groups = multiplexer.GetDatabase().StreamGroupInfo(key);
            if (!groups.Any(g => g.Name == name))
            {
                // group doesn't exist, assume all entries in stream have not been processed
                return new RedisPollingTriggerBaseMetrics
                {
                    Remaining = streamLength,
                    Timestamp = DateTime.UtcNow,
                };
            }

            StreamGroupInfo group = groups.First(g => g.Name == name);
            if (multiplexer.GetServers()[0].Version >= RedisUtilities.Version70 && group.Lag.HasValue)
            {
                // Redis 7: Scaler gets number of remaining entries for the consumer group from XINFO GROUPS.
                return new RedisPollingTriggerBaseMetrics
                {
                    Remaining = group.Lag.Value,
                    Timestamp = DateTime.UtcNow,
                };
            }

            /*
             * Redis 6/6.2 does not have an internal counter for the number of remaining entries for the consumer group to read
             * Estimate number of remaining entries in the stream using the unixtime field from the entry IDs:
             * - default ID structure ("unixtime-counter")
             * - entries are added at a constant rate over time
            */
            StreamInfo info = await multiplexer.GetDatabase().StreamInfoAsync(key);
            string firstId = (string)info.FirstEntry.Id;
            string lastId = (string)info.LastEntry.Id;
            string lastDeliveredId = group.LastDeliveredId ?? firstId;

            long firstTimestamp = long.Parse(firstId.Split('-')[0]);
            long lastTimestamp = long.Parse(lastId.Split('-')[0]);
            long lastDeliveredTimestamp = long.Parse(lastDeliveredId.Split('-')[0]);

            decimal timeRemaining = lastTimestamp - lastDeliveredTimestamp;
            decimal timeTotal = lastTimestamp - firstTimestamp;
            decimal percentageRemaining = timeRemaining / timeTotal;
            long estimatedRemaining = Math.Min(streamLength, (long)Math.Ceiling(percentageRemaining * streamLength));

            return new RedisPollingTriggerBaseMetrics
            {
                Remaining = estimatedRemaining,
                Timestamp = DateTime.UtcNow,
            };
        }
    }
}
