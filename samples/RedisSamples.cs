using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhost = "127.0.0.1:6379";

        [FunctionName("PubSubTrigger")]
        public static void PubSubTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.PubSub, Trigger = "maran")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyspaceTrigger")]
        public static void KeyspaceTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeySpace, Trigger = "maran2")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyeventTrigger")]
        public static void KeyeventTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeyEvent, Trigger = "del")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyspaceTriggerInputBinding")]
        public static void KeyspaceTriggerInputBinding(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeyEvent, Trigger = "set")] RedisMessageModel result,
            [Redis(ConnectionString = localhost)] IConnectionMultiplexer connectionMultiplexer,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {result.Trigger} for key {result.Message}");
            logger.LogInformation($"Value of key {result.Message} = {connectionMultiplexer.GetDatabase().StringGet(result.Message)}");
            logger.LogInformation(JsonSerializer.Serialize(result));
        }
    }
}
