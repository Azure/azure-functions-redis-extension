using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        [FunctionName("PubSubTrigger")]
        public static void PubSubTrigger(
            [RedisTrigger(ConnectionString = "127.0.0.1:6379", TriggerType = RedisTriggerType.PubSub, Trigger = "maran")]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyspaceTrigger")]
        public static void KeyspaceTrigger(
            [RedisTrigger(ConnectionString = "127.0.0.1:6379", TriggerType = RedisTriggerType.KeySpace, Trigger = "maran2")]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyeventTrigger")]
        public static void KeyeventTrigger(
            [RedisTrigger(ConnectionString = "127.0.0.1:6379", TriggerType = RedisTriggerType.KeyEvent, Trigger = "del")]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }
    }
}
