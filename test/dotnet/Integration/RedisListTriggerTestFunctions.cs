using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisListTriggerTestFunctions
    {
        public const int pollingIntervalShort = 100;
        public const int pollingIntervalLong = 10000;
        public const int batchSize = 10;

        [FunctionName(nameof(ListTrigger_RedisValue))]
        public static void ListTrigger_RedisValue(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_RedisValue), pollingIntervalInMs: pollingIntervalShort)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_String))]
        public static void ListTrigger_String(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_String), pollingIntervalInMs: pollingIntervalShort)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_ByteArray))]
        public static void ListTrigger_ByteArray(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_ByteArray), pollingIntervalInMs: pollingIntervalShort)] byte[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_CustomType))]
        public static void ListTrigger_CustomType(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_CustomType), pollingIntervalInMs: pollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_Batch_RedisValue))]
        public static void ListTrigger_Batch_RedisValue(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_RedisValue), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] RedisValue[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_Batch_String))]
        public static void ListTrigger_Batch_String(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_String), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] string[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_Batch_ByteArray))]
        public static void ListTrigger_Batch_ByteArray(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_ByteArray), pollingIntervalInMs: pollingIntervalShort, maxBatchSize: batchSize)] byte[][] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }

        [FunctionName(nameof(ListTrigger_RedisValue_LongPollingInterval))]
        public static void ListTrigger_RedisValue_LongPollingInterval(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_RedisValue_LongPollingInterval), pollingIntervalInMs: pollingIntervalLong, maxBatchSize: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
