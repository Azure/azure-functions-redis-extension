using Microsoft.Azure.WebJobs.Host.Scale;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisListTriggerScaleMonitor : RedisPollingTriggerBaseScaleMonitor
    {
        public RedisListTriggerScaleMonitor(IConnectionMultiplexer multiplexer,
            string name,
            int maxBatchSize,
            string key) 
            : base(multiplexer, maxBatchSize, key)
        {
            this.Descriptor = new ScaleMonitorDescriptor(name, $"{name}-RedisListTrigger-{key}");
            this.TargetScalerDescriptor = new TargetScalerDescriptor($"{name}-RedisListTrigger-{key}");
        }

        public override Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            var metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = multiplexer.GetDatabase().ListLength(key),
                Timestamp = DateTime.UtcNow,
            };

            return Task.FromResult(metrics);
        }
    }
}
