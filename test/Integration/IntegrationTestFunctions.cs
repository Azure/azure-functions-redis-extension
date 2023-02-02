using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string connectionString = "127.0.0.1:6379";
        public const string pubsubChannel = "testChannel";
        public const string keyspaceKey = "testKey";
        public const string keyeventEvent = "set";
        public const string all = "*";

        [FunctionName(nameof(PubSubTrigger_SingleChannel))]
        public static void PubSubTrigger_SingleChannel(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "PubSub", Trigger = pubsubChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "PubSub", Trigger = pubsubChannel + all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "PubSub", Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "KeySpace", Trigger = keyspaceKey)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "KeySpace", Trigger = keyspaceKey + all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "KeySpace", Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "KeyEvent", Trigger = keyeventEvent)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = "KeyEvent", Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
