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

        [FunctionName(nameof(PubSubTrigger_SingleChannel))]
        public static void PubSubTrigger_SingleChannel(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel + all)] RedisMessageModel model,
        ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = all)] RedisMessageModel model,
        ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = keyspaceKey + all)] RedisMessageModel model,
        ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
        [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeySpace, Trigger = all)] RedisMessageModel model,
        ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = keyeventEvent)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.KeyEvent, Trigger = all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ConnectionBinding))]
        public static void ConnectionBinding(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
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
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            [RedisCommand(ConnectionString = connectionString, RedisCommand = "set", Arguments = bindingKey + " " + bindingValue + "1")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ScriptBinding))]
        public static void ScriptBinding(
            [RedisTrigger(ConnectionString = connectionString, TriggerType = RedisTriggerType.PubSub, Trigger = pubsubChannel)] RedisMessageModel model,
            [RedisScript(ConnectionString = connectionString, LuaScript = "return redis.call('SET', KEYS[1], ARGV[1])", Keys = bindingKey, Values = bindingValue + "2")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
