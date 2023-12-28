using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const int batchSize = 10;
        public const int pollingIntervalShort = 100;
        public const int pollingIntervalLong = 10000;

        [FunctionName(nameof(StreamTrigger_StreamEntry))]
        public static void StreamTrigger_StreamEntry(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_StreamEntry), pollingIntervalInMs: pollingIntervalShort)] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_NameValueEntryArray))]
        public static void StreamTrigger_NameValueEntryArray(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_NameValueEntryArray), pollingIntervalInMs: pollingIntervalShort)] NameValueEntry[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Dictionary))]
        public static void StreamTrigger_Dictionary(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Dictionary), pollingIntervalInMs: pollingIntervalShort)] Dictionary<string, string> values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void StreamTrigger_ByteArray(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_ByteArray), pollingIntervalInMs: pollingIntervalShort)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_String))]
        public static void StreamTrigger_String(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_String), pollingIntervalInMs: pollingIntervalShort)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_CustomType))]
        public static void StreamTrigger_CustomType(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_CustomType), pollingIntervalInMs: pollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_RedisValue_LongPollingInterval))]
        public static void StreamTrigger_RedisValue_LongPollingInterval(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_RedisValue_LongPollingInterval), pollingIntervalInMs: pollingIntervalLong, maxBatchSize: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_StreamEntry))]
        public static void StreamTrigger_Batch_StreamEntry(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_StreamEntry), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] StreamEntry[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_NameValueEntryArray))]
        public static void StreamTrigger_Batch_NameValueEntryArray(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_NameValueEntryArray), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] NameValueEntry[][] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(StreamTrigger_Batch_Dictionary))]
        public static void StreamTrigger_Batch_Dictionary(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_Dictionary), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] Dictionary<string, string>[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Batch_ByteArray))]
        public static void StreamTrigger_Batch_ByteArray(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_ByteArray), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] byte[][] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }

        [FunctionName(nameof(StreamTrigger_Batch_String))]
        public static void StreamTrigger_Batch_String(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_String), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] string[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
