using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Batch_ByteArray
    {
        [FunctionName(nameof(ListTrigger_Batch_ByteArray))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_ByteArray), IntegrationTestHelpers.pollingIntervalShort, IntegrationTestHelpers.batchSize)] byte[][] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
