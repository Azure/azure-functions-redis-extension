using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisListTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingInterval = 100;

        [FunctionName(nameof(ListTrigger_RedisValue))]
        public static void ListTrigger_RedisValue(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_RedisValue), pollingIntervalInMs: pollingInterval)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_String))]
        public static void ListTrigger_String(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_String), pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_ByteArray))]
        public static void ListTrigger_ByteArray(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_ByteArray), pollingIntervalInMs: pollingInterval)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_CustomType))]
        public static void ListTrigger_CustomType(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_CustomType), pollingIntervalInMs: pollingInterval)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
