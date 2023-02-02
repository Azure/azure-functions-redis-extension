using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhost = "127.0.0.1:6379";

        [FunctionName("PubSubTrigger")]
        public static void PubSubTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.PubSub, Trigger = "maran")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName("KeyspaceTrigger")]
        public static void KeyspaceTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeySpace, Trigger = "maran2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName("KeyeventTrigger")]
        public static void KeyeventTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeyEvent, Trigger = "del")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
