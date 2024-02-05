using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Batch_CustomTypeArray
    {
        [FunctionName(nameof(ListTrigger_Single_CustomType))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(ListTrigger_Single_CustomType), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] CustomType[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
