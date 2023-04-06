using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamsTriggerTestFunctions
    {
        public const string stringFormat = "streamEntry:{0}";
        public const string localhostSetting = "redisLocalhost";
        public const string streamSingleKey = "streamSingleKey";
        public const string streamMultipleKeys = "streamKey1 streamKey2 streamKey3";
        public const string alphabet = "a b c d e f g h i j k l m n o p q r s t u v w x y z";
        public const int pollingInterval = 100;
        public const int count = 100;

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
