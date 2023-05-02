using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(StreamTrigger_StreamEntry))]
        public static void StreamTrigger_StreamEntry(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_StreamEntry), pollingIntervalInMs: pollingInterval)] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_NameValueEntryArray))]
        public static void StreamTrigger_NameValueEntryArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_NameValueEntryArray), pollingIntervalInMs: pollingInterval)] NameValueEntry[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void StreamTrigger_Dictionary(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Dictionary), pollingIntervalInMs: pollingInterval)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void StreamTrigger_ByteArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_ByteArray), pollingIntervalInMs: pollingInterval)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_String))]
        public static void StreamTrigger_String(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_String), pollingIntervalInMs: pollingInterval)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_CustomType))]
        public static void StreamTrigger_CustomType(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_CustomType), pollingIntervalInMs: pollingInterval)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
