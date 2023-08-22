using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisListTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingIntervalShort = 100;
        public const int pollingIntervalLong = 10000;

        [FunctionName(nameof(ListTrigger_RedisValue))]
        public static void ListTrigger_RedisValue(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_RedisValue), pollingIntervalInMs: pollingIntervalShort)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_String))]
        public static void ListTrigger_String(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_String), pollingIntervalInMs: pollingIntervalShort)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_ByteArray))]
        public static void ListTrigger_ByteArray(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_ByteArray), pollingIntervalInMs: pollingIntervalShort)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_CustomType))]
        public static void ListTrigger_CustomType(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_CustomType), pollingIntervalInMs: pollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_RedisValue_LongPollingInterval))]
        public static void ListTrigger_RedisValue_LongPollingInterval(
            [RedisListTrigger(localhostSetting, nameof(ListTrigger_RedisValue_LongPollingInterval), pollingIntervalInMs: pollingIntervalLong, messagesPerWorker: 1, count: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
