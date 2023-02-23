using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string pubsubChannel = "testChannel";
        public const string keyspaceChannel = "__keyspace@0__:testKey";
        public const string keyeventChannel = "__keyevent@0__:set";
        public const string keyeventChannelAll = "__keyevent@0__:*";
        public const string keyspaceChannelAll = "__keyspace@0__:*";
        public const string all = "*";
        public const string listSingleKey = "listSingleKey";
        public const string listMultipleKeys = "listKey1 listKey2 listKey3";
        public const string streamSingleKey = "streamSingleKey";
        public const string streamMultipleKeys = "streamKey1 streamKey2 streamKey3";
        public const int pollingInterval = 100;
        public const string bindingKey = "bindingKey";
        public const string bindingValue = "bindingValue";
        public const int count = 100;

        [FunctionName(nameof(PubSubTrigger_SingleChannel))]
        public static void PubSubTrigger_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel + "*")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
            [RedisPubSubTrigger(localhostSetting, all)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel + "*")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
        [FunctionName(nameof(ListsTrigger_SingleKey))]
        public static void ListsTrigger_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(ListsTrigger_MultipleKeys))]
        public static void ListsTrigger_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_SingleKey))]
        public static void StreamsTrigger_DefaultGroup_SingleKey(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_MultipleKeys))]
        public static void StreamsTrigger_DefaultGroup_MultipleKeys(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(CommandBinding))]
        public static void CommandBinding(
            [RedisPubSubTrigger(ConnectionString = connectionString, Channel = pubsubChannel)] RedisMessageModel model,
            [RedisCommand(ConnectionString = connectionString, Command = "set", Args = bindingKey + " " + bindingValue + "1")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ScriptBinding))]
        public static void ScriptBinding(
            [RedisPubSubTrigger(ConnectionString = connectionString, Channel = pubsubChannel)] RedisMessageModel model,
            [RedisScript(ConnectionString = connectionString, Script = "return redis.call('SET', KEYS[1], ARGV[1])", Keys = bindingKey, Args = bindingValue + "2")] RedisResult result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

    }
}
