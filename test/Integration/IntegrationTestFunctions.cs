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
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel + all)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = all)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey + all)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = all)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = keyeventEvent)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = all)]
            RedisMessageModel result, ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }
    }
}
