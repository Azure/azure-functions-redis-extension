using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string triggerValueFormat = "triggerValue:{0}";
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
        public const int count = 100;

        [FunctionName(nameof(PubSubTrigger_SingleChannel))]
        public static void PubSubTrigger_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(PubSubTrigger_SingleChannel_RedisTriggerModel))]
        public static void PubSubTrigger_SingleChannel_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel + "*")] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels_RedisTriggerModel))]
        public static void PubSubTrigger_MultipleChannels_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel + "*")] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
            [RedisPubSubTrigger(localhostSetting, all)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels_RedisTriggerModel))]
        public static void PubSubTrigger_AllChannels_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, all)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey_RedisTriggerModel))]
        public static void KeySpaceTrigger_SingleKey_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel + "*")] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys_RedisTriggerModel))]
        public static void KeySpaceTrigger_MultipleKeys_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel + "*")] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys_RedisTriggerModel))]
        public static void KeySpaceTrigger_AllKeys_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent_RedisTriggerModel))]
        public static void KeyEventTrigger_SingleEvent_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, message));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents_RedisTriggerModel))]
        public static void KeyEventTrigger_AllEvents_RedisTriggerModel(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] RedisTriggerModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ListsTrigger_SingleKey))]
        public static void ListsTrigger_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] string element,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, element));
        }

        [FunctionName(nameof(ListsTrigger_SingleKey_RedisTriggerModel))]
        public static void ListsTrigger_SingleKey_RedisTriggerModel(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] RedisTriggerModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(ListsTrigger_MultipleKeys))]
        public static void ListsTrigger_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] string element,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, element));
        }

        [FunctionName(nameof(ListsTrigger_MultipleKeys_RedisTriggerModel))]
        public static void ListsTrigger_MultipleKeys_RedisTriggerModel(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisTriggerModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_SingleKey))]
        public static void StreamsTrigger_DefaultGroup_SingleKey(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] IReadOnlyDictionary<string, string> entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_SingleKey_RedisTriggerModel))]
        public static void StreamsTrigger_DefaultGroup_SingleKey_RedisTriggerModel(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] RedisTriggerModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_MultipleKeys))]
        public static void StreamsTrigger_DefaultGroup_MultipleKeys(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] IReadOnlyDictionary<string, string> entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(triggerValueFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(StreamsTrigger_DefaultGroup_MultipleKeys_RedisTriggerModel))]
        public static void StreamsTrigger_DefaultGroup_MultipleKeys_RedisTriggerModel(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisTriggerModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }
    }
}
