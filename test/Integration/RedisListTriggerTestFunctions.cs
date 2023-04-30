using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisListTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string listKey = "listKey";
        public const int pollingInterval = 100;

        [FunctionName(nameof(ListTriggerRedisValue))]
        public static void ListTriggerRedisValue(
            [RedisListTrigger(localhostSetting, listKey, pollingIntervalInMs: pollingInterval)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTriggerString))]
        public static void ListTriggerString(
            [RedisListTrigger(localhostSetting, listKey, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTriggerByteArray))]
        public static void ListTriggerByteArray(
            [RedisListTrigger(localhostSetting, listKey, pollingIntervalInMs: pollingInterval)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
