using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisListTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingInterval = 100;

        [FunctionName(nameof(ListTriggerRedisValue))]
        public static void ListTriggerRedisValue(
            [RedisListTrigger(localhostSetting, nameof(ListTriggerRedisValue), pollingIntervalInMs: pollingInterval)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTriggerString))]
        public static void ListTriggerString(
            [RedisListTrigger(localhostSetting, nameof(ListTriggerString), pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTriggerByteArray))]
        public static void ListTriggerByteArray(
            [RedisListTrigger(localhostSetting, nameof(ListTriggerByteArray), pollingIntervalInMs: pollingInterval)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
