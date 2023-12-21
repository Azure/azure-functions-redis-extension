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
            if (multiplexer.GetServers()[0].Version <= RedisUtilities.Version70)
            {
                StreamGroupInfo[] groups = multiplexer.GetDatabase().StreamGroupInfo(key);
                entriesRemaining = groups.Where(group => group.Name == name).First().Lag ?? 0;
            }
            else
            {
                long length = await multiplexer.GetDatabase().StreamLengthAsync(key);
                long processed = (long) await multiplexer.GetDatabase().StringGetAsync(RedisScalerProvider.GetFunctionScalerId(name, "RedisStreamTrigger", key));
                entriesRemaining = length - processed;
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
