using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Batch_String
    {
        [FunctionName(nameof(ListTrigger_Batch_String))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_String), IntegrationTestHelpers.pollingIntervalShort, IntegrationTestHelpers.batchSize)] string[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
