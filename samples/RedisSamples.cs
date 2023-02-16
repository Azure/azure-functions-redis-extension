using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhostSetting = "redisLocalhost";

        [FunctionName(nameof(PubSubTrigger))]
        public static void PubSubTrigger(
            [RedisPubSubTrigger(localhostSetting, "pubsubTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTriggerResolvedChannel))]
        public static void PubSubTriggerResolvedChannel(
            [RedisPubSubTrigger(localhostSetting, "%pubsubChannel%")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyspaceTrigger))]
        public static void KeyspaceTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyspace@0__:keyspaceTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyeventTrigger))]
        public static void KeyeventTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:del")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyspaceTriggerCommandBinding))]
        public static void KeyspaceTriggerCommandBinding(
            [RedisPubSubTrigger(ConnectionString = localhost, Channel = "__keyspace@0__:keytest")] RedisMessageModel model,
            [RedisCommand(ConnectionString = localhost, RedisCommand = "get", Arguments = "keytest")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {model.Message} event for key {model.Trigger}");
            logger.LogInformation($"Value of key {model.Message} = {result}");
        }

        [FunctionName(nameof(KeyspaceTriggerScriptBinding))]
        public static void KeyspaceTriggerScriptBinding(
            [RedisPubSubTrigger(ConnectionString = localhost, Channel = "__keyspace@0__:scriptTest")] RedisMessageModel model,
            [RedisScript(ConnectionString = localhost, LuaScript = "return redis.call('GET', KEYS[1])", Keys = "scriptTest")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {model.Message} event for key {model.Trigger}");
            logger.LogInformation($"Value of key {model.Message} = {result}");
        }

        [FunctionName(nameof(ListsTrigger))]
        public static void ListsTrigger(
            [RedisListsTrigger(localhostSetting, "listTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ListsMultipleTrigger))]
        public static void ListsMultipleTrigger(
            [RedisListsTrigger(localhostSetting, "listTest1 listTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(StreamsTrigger))]
        public static void StreamsTrigger(
            [RedisStreamsTrigger(localhostSetting, "streamTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(StreamsMultipleTriggers))]
        public static void StreamsMultipleTriggers(
            [RedisStreamsTrigger(localhostSetting, "streamTest1 streamTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
