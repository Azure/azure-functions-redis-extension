using Microsoft.Azure.WebJobs.Host.Scale;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisPollingTriggerBaseMetrics : ScaleMetrics
    {
        public long Remaining { get; set; }
    }
}
