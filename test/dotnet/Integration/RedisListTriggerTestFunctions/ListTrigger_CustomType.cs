using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_CustomType
    {
        [FunctionName(nameof(ListTrigger_CustomType))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(ListTrigger_CustomType), IntegrationTestHelpers.PollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
