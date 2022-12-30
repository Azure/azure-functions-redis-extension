using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// This model gets returned when the function is triggered.
    /// </summary>
    public class RedisMessageModel
    {
        public RedisTriggerType TriggerType { get; set; }
        public string Trigger { get; set; }
        public string[] Message { get; set; }
    }
}
