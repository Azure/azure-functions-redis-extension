using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisListsTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string listSingleKey = "listSingleKey";
        public const string listMultipleKeys = "listKey1 listKey2 listKey3";
        public const int pollingInterval = 100;

        [FunctionName(nameof(ListsTrigger_RedisListEntry_SingleKey))]
        public static void ListsTrigger_RedisListEntry_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }

        [FunctionName(nameof(ListsTrigger_string_SingleKey))]
        public static void ListsTrigger_string_SingleKey(
            [RedisListsTrigger(localhostSetting, listSingleKey, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }

        [FunctionName(nameof(ListsTrigger_RedisListEntry_MultipleKeys))]
        public static void ListsTrigger_RedisListEntry_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] RedisListEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }

        [FunctionName(nameof(ListsTrigger_string_MultipleKeys))]
        public static void ListsTrigger_string_MultipleKeys(
            [RedisListsTrigger(localhostSetting, listMultipleKeys, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}
