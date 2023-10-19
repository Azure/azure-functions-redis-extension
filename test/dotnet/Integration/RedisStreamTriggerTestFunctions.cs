using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingIntervalShort = 100;
        public const int pollingIntervalLong = 10000;

        [FunctionName(nameof(StreamTrigger_StreamEntry))]
        public static void StreamTrigger_StreamEntry(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_StreamEntry), pollingIntervalInMs: pollingIntervalShort)] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_NameValueEntryArray))]
        public static void StreamTrigger_NameValueEntryArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_NameValueEntryArray), pollingIntervalInMs: pollingIntervalShort)] NameValueEntry[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void StreamTrigger_Dictionary(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Dictionary), pollingIntervalInMs: pollingIntervalShort)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void StreamTrigger_ByteArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_ByteArray), pollingIntervalInMs: pollingIntervalShort)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_String))]
        public static void StreamTrigger_String(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_String), pollingIntervalInMs: pollingIntervalShort)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_CustomType))]
        public static void StreamTrigger_CustomType(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_CustomType), pollingIntervalInMs: pollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_RedisValue_LongPollingInterval))]
        public static void StreamTrigger_RedisValue_LongPollingInterval(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_RedisValue_LongPollingInterval), pollingIntervalInMs: pollingIntervalLong, maxBatchSize: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

    }
}
