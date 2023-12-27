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
            long entriesRemaining = 0;
            long streamLength = await multiplexer.GetDatabase().StreamLengthAsync(key);
            StreamGroupInfo group = multiplexer.GetDatabase().StreamGroupInfo(key).Where(g => g.Name == name).First();
            long? lag = group.Lag;

            if (lag.HasValue)
            {
                // Redis 7: Scaler gets number of unacked stream entries for the consumer group from XINFO GROUPS.
                entriesRemaining = lag.Value;
            }
            else
            {
                /* Redis 6/6.2 does not have an internal counter for the number of remaining entries for the consumer group to read
                 * We estimate number of remaining entries in the stream, assuming the following:
                 * - default ID structure ("unixtime-counter")
                 * - entries are added at a constant rate over time
                 */
                StreamInfo info = await multiplexer.GetDatabase().StreamInfoAsync(key);
                string firstId = (string)info.FirstEntry.Id;
                string lastId = (string)info.LastEntry.Id;
                string lastDeliveredId = group.LastDeliveredId;

                long firstTimestamp = long.Parse(firstId.Split('-')[0]);
                long lastTimestamp = long.Parse(lastId.Split('-')[0]);
                long lastDeliveredTimestamp = long.Parse(lastDeliveredId.Split('-')[0]);

                decimal timeRemaining = lastTimestamp - lastDeliveredTimestamp;
                decimal timeTotal = lastTimestamp - firstTimestamp;
                decimal percentageRemaining = timeRemaining / timeTotal;

                entriesRemaining = (long) Math.Ceiling(percentageRemaining * streamLength);
            }

            RedisPollingTriggerBaseMetrics metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = entriesRemaining,
                Timestamp = DateTime.UtcNow,
            };

            return metrics;
        }
    }
}
