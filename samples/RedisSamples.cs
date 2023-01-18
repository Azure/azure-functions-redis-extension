using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhost = "127.0.0.1:6379";

        [FunctionName(nameof(PubSubTrigger))]
        public static void PubSubTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.PubSub), Trigger = "pubsubTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyspaceTrigger))]
        public static void KeyspaceTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.KeySpace), Trigger = "keyspaceTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyeventTrigger))]
        public static void KeyeventTrigger(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.KeyEvent), Trigger = "del")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyeventTriggerConnectionBinding))]
        public static void KeyeventTriggerConnectionBinding(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.KeyEvent), Trigger = "set")] RedisMessageModel model,
            [RedisConnection(ConnectionString = localhost)] IConnectionMultiplexer connectionMultiplexer,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
            logger.LogInformation($"Triggered on {model.Trigger} for key {model.Message}");
            logger.LogInformation($"Value of key {model.Message} = {connectionMultiplexer.GetDatabase().StringGet(model.Message)}");
            connectionMultiplexer.Close();
            connectionMultiplexer.Dispose();
        }

        [FunctionName(nameof(KeyspaceTriggerCommandBinding))]
        public static void KeyspaceTriggerCommandBinding(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.KeySpace), Trigger = "keytest")] RedisMessageModel model,
            [RedisCommand(ConnectionString = localhost, RedisCommand = "get", Arguments = "keytest")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {model.Message} event for key {model.Trigger}");
            logger.LogInformation($"Value of key {model.Message} = {result}");
        }

        [FunctionName(nameof(KeyspaceTriggerScriptBinding))]
        public static void KeyspaceTriggerScriptBinding(
            [RedisPubSubTrigger(ConnectionString = localhost, TriggerType = nameof(RedisTriggerType.KeySpace), Trigger = "scriptTest")] RedisMessageModel model,
            [RedisScript(ConnectionString = localhost, LuaScript = "return redis.call('GET', KEYS[1])", Keys = "scriptTest")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {model.Message} event for key {model.Trigger}");
            logger.LogInformation($"Value of key {model.Message} = {result}");
        }

        [FunctionName(nameof(StreamsTrigger))]
        public static void StreamsTrigger(
            [RedisStreamsTrigger(ConnectionString = localhost, Keys = "streamTest", PollingInterval = "1000")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(StreamsMultipleTriggers))]
        public static void StreamsMultipleTriggers(
            [RedisStreamsTrigger(ConnectionString = localhost, Keys = "streamTest1 streamTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ListsTrigger))]
        public static void ListsTrigger(
            [RedisListsTrigger(ConnectionString = localhost, Keys = "listTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ListsMultipleTrigger))]
        public static void ListsMultipleTrigger(
            [RedisListsTrigger(ConnectionString = localhost, Keys = "listTest1 listTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
