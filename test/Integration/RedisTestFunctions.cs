using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisTestFunctions
    {
        public const string stringFormat = "entry:{0}";
        public const string localhostSetting = "redisLocalhost";
        public const string all = "*";
        public const string pubsubChannel = "testChannel";
        public const string pubsubPattern = "testChannel*";
        public const string keyspaceChannel = "__keyspace@0__:testKey";
        public const string keyspacePattern = "__keyspace@0__:testKey*";
        public const string keyeventChannel = "__keyevent@0__:set";
        public const string keyeventChannelAll = "__keyevent@0__:*";
        public const string keyspaceChannelAll = "__keyspace@0__:*";
        public const string listSingleKey = "listSingleKey";
        public const string listMultipleKeys = "listKey1 listKey2 listKey3";
        public const string streamSingleKey = "streamSingleKey";
        public const string streamMultipleKeys = "streamKey1 streamKey2 streamKey3";
        public const string alphabet = "a b c d e f g h i j k l m n o p q r s t u v w x y z";
        public const int pollingInterval = 100;

        [FunctionName(nameof(PubSubTrigger_RedisPubSubMessage_SingleChannel))]
        public static void PubSubTrigger_RedisPubSubMessage_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(PubSubTrigger_string_SingleChannel))]
        public static void PubSubTrigger_string_SingleChannel(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(PubSubTrigger_RedisPubSubMessage_ChannelPattern))]
        public static void PubSubTrigger_RedisPubSubMessage_ChannelPattern(
            [RedisPubSubTrigger(localhostSetting, pubsubPattern)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(PubSubTrigger_string_ChannelPattern))]
        public static void PubSubTrigger_string_ChannelPattern(
            [RedisPubSubTrigger(localhostSetting, pubsubPattern)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(PubSubTrigger_RedisPubSubMessage_AllChannels))]
        public static void PubSubTrigger_RedisPubSubMessage_AllChannels(
            [RedisPubSubTrigger(localhostSetting, all)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }
        [FunctionName(nameof(PubSubTrigger_string_AllChannels))]
        public static void PubSubTrigger_string_AllChannels(
            [RedisPubSubTrigger(localhostSetting, all)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_RedisPubSubMessage_SingleKey))]
        public static void KeySpaceTrigger_RedisPubSubMessage_SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(KeySpaceTrigger_string_SingleKey))]
        public static void KeySpaceTrigger_string_SingleKey(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_RedisPubSubMessage_MultipleKeys))]
        public static void KeySpaceTrigger_RedisPubSubMessage_MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspacePattern)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(KeySpaceTrigger_string_MultipleKeys))]
        public static void KeySpaceTrigger_string_MultipleKeys(
            [RedisPubSubTrigger(localhostSetting, keyspacePattern)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(KeySpaceTrigger_RedisPubSubMessage_AllKeys))]
        public static void KeySpaceTrigger_RedisPubSubMessage_AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(KeySpaceTrigger_string_AllKeys))]
        public static void KeySpaceTrigger_string_AllKeys(
            [RedisPubSubTrigger(localhostSetting, keyspaceChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(KeyEventTrigger_RedisPubSubMessage_SingleEvent))]
        public static void KeyEventTrigger_RedisPubSubMessage_SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(KeyEventTrigger_string_SingleEvent))]
        public static void KeyEventTrigger_string_SingleEvent(
            [RedisPubSubTrigger(localhostSetting, keyeventChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(KeyEventTrigger_RedisPubSubMessage_AllEvents))]
        public static void KeyEventTrigger_RedisPubSubMessage_AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(message)));
        }

        [FunctionName(nameof(KeyEventTrigger_string_AllEvents))]
        public static void KeyEventTrigger_string_AllEvents(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, message));
        }

        [FunctionName(nameof(ListsTrigger_RedisListEntry_SingleKey))]
        public static void ListsTrigger_RedisListEntry_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(ListsTrigger_string_SingleKey))]
        public static void ListsTrigger_string_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, entry));
        }

        [FunctionName(nameof(ListsTrigger_RedisListEntry_MultipleKeys))]
        public static void ListsTrigger_RedisListEntry_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(ListsTrigger_string_MultipleKeys))]
        public static void ListsTrigger_string_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, entry));
        }

        [FunctionName(nameof(StreamsTrigger_RedisStreamEntry_SingleKey))]
        public static void StreamsTrigger_RedisStreamEntry_SingleKey(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] RedisStreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(StreamsTrigger_KeyValuePair_SingleKey))]
        public static void StreamsTrigger_KeyValuePair_SingleKey(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] KeyValuePair<string, string>[] values,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(values)));
        }

        [FunctionName(nameof(StreamsTrigger_IReadOnlyDictionary_SingleKey))]
        public static void StreamsTrigger_IReadOnlyDictionary_SingleKey(
            [RedisStreamsTrigger(localhostSetting, streamSingleKey, pollingIntervalInMs: pollingInterval)] IReadOnlyDictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(values)));
        }

        [FunctionName(nameof(StreamsTrigger_RedisStreamEntry_MultipleKeys))]
        public static void StreamsTrigger_RedisStreamEntry_MultipleKeys(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisStreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(entry)));
        }

        [FunctionName(nameof(StreamsTrigger_KeyValuePair_MultipleKeys))]
        public static void StreamsTrigger_KeyValuePair_MultipleKeys(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] KeyValuePair<string, string>[] values,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(values)));
        }

        [FunctionName(nameof(StreamsTrigger_IReadOnlyDictionary_MultipleKeys))]
        public static void StreamsTrigger_IReadOnlyDictionary_MultipleKeys(
            [RedisStreamsTrigger(localhostSetting, streamMultipleKeys, pollingIntervalInMs: pollingInterval)] IReadOnlyDictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(string.Format(stringFormat, JsonSerializer.Serialize(values)));
        }
    }
}
