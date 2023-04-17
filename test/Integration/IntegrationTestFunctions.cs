using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string format = "triggerValue:{0}";
        public const string pubsubChannel = "testChannel";
        public const string pubsubMultiple = "testChannel*";
        public const string keyspaceChannel = "__keyspace@0__:testKey";
        public const string keyspaceMultiple = "__keyspace@0__:testKey*";
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
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(PubSubTrigger_MultipleChannels))]
        public static void PubSubTrigger_MultipleChannels(
            [RedisPubSubTrigger(localhostSetting, pubsubMultiple)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(PubSubTrigger_AllChannels))]
        public static void PubSubTrigger_AllChannels(
            [RedisPubSubTrigger(localhostSetting, all)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeySpaceTrigger_SingleKey))]
        public static void KeySpaceTrigger_SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeySpaceTrigger_MultipleKeys))]
        public static void KeySpaceTrigger_MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceMultiple)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeySpaceTrigger_AllKeys))]
        public static void KeySpaceTrigger_AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeyEventTrigger_SingleEvent))]
        public static void KeyEventTrigger_SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeyEventTrigger_AllEvents))]
        public static void KeyEventTrigger_AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(ListTrigger_SingleKey))]
        public static void ListTrigger_SingleKey(
            [RedisListTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }

        [FunctionName(nameof(ListsTrigger_MultipleKeys))]
        public static void ListsTrigger_MultipleKeys(
            [RedisListTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }

        [FunctionName(nameof(StreamsTrigger_SingleKey))]
        public static void StreamsTrigger_SingleKey(
            [RedisStreamTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] RedisStreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }

        [FunctionName(nameof(StreamsTrigger_MultipleKeys))]
        public static void StreamsTrigger_MultipleKeys(
            [RedisStreamTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisStreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }
    }
}
