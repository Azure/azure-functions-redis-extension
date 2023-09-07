using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const int pollingInterval = 100;
        public const int batchSize = 10;

        [FunctionName(nameof(StreamTrigger_StreamEntry))]
        public static void StreamTrigger_StreamEntry(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_StreamEntry), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_NameValueEntryArray))]
        public static void StreamTrigger_NameValueEntryArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_NameValueEntryArray), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] NameValueEntry[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void StreamTrigger_Dictionary(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Dictionary), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void StreamTrigger_ByteArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_ByteArray), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_String))]
        public static void StreamTrigger_String(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_String), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_CustomType))]
        public static void StreamTrigger_CustomType(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_CustomType), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_StreamEntry))]
        public static void StreamTrigger_Batch_StreamEntry(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Batch_StreamEntry), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] StreamEntry[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_NameValueEntryArray))]
        public static void StreamTrigger_Batch_NameValueEntryArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Batch_NameValueEntryArray), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] NameValueEntry[][] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_Dictionary))]
        public static void StreamTrigger_Batch_Dictionary(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Batch_Dictionary), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] Dictionary<string, string>[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Batch_ByteArray))]
        public static void StreamTrigger_Batch_ByteArray(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Batch_ByteArray), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] byte[][] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Batch_String))]
        public static void StreamTrigger_Batch_String(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTrigger_Batch_String), pollingIntervalInMs: pollingInterval, maxBatchSize: batchSize)] string[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
