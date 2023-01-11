using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;


namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string connectionString = "127.0.0.1:6379";
        public const string pubsubChannel = "testChannel";
        public const string keyspaceKey = "testKey";
        public const string keyeventEvent = "set";
        public const string all = "*";
        public const string bindingKey = "bindingKey";
        public const string bindingValue = "bindingValue";
        public const string streamSingleKey = "streamSingleKey";
        public const string streamMultipleKeys = "streamKey1 streamKey2 streamKey3";
        public const string consumerGroup = "consumerGroup";
        public const string consumerName = "consumerName";
        public const string listSingleKey = "listSingleKey";
        public const string listMultipleKeys = "listKey1 listKey2 listKey3";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(PubSubTrigger_SingleChannel))]
        public static void PubSubTrigger_SingleChannel(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel + all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey + all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = keyeventEvent)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ConnectionBinding))]
        public static void ConnectionBinding(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            [RedisConnection(ConnectionString = connectionString)] IConnectionMultiplexer multiplexer,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
            multiplexer.GetDatabase().StringSet(bindingKey, bindingValue);
            multiplexer.Close();
            multiplexer.Dispose();
        }

        [FunctionName(nameof(CommandBinding))]
        public static void CommandBinding(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            [RedisCommand(ConnectionString = connectionString, RedisCommand = "set", Arguments = bindingKey + " " + bindingValue + "1")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ScriptBinding))]
        public static void ScriptBinding(
            [RedisPubSubTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            [RedisScript(ConnectionString = connectionString, LuaScript = "return redis.call('SET', KEYS[1], ARGV[1])", Keys = bindingKey, Values = bindingValue + "2")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(ListsTrigger_SingleKey))]
        public static void ListsTrigger_SingleKey(
            [RedisListsTrigger(ConnectionString = connectionString, Keys = listSingleKey, PollingInterval = pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(ListsTrigger_MultipleKeys))]
        public static void ListsTrigger_MultipleKeys(
            [RedisListsTrigger(ConnectionString = connectionString, Keys = listMultipleKeys, PollingInterval = pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_WithoutGroup_SingleKey))]
        public static void StreamsTrigger_WithoutGroup_SingleKey(
            [RedisStreamsTrigger(ConnectionString = connectionString, Keys = streamSingleKey, PollingInterval = pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_WithoutGroup_MultipleKeys))]
        public static void StreamsTrigger_WithoutGroup_MultipleKeys(
            [RedisStreamsTrigger(ConnectionString = connectionString, Keys = streamMultipleKeys, PollingInterval = pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }
    }
}
