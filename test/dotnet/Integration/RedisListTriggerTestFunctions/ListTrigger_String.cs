using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_String
    {
        [FunctionName(nameof(ListTrigger_String))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(ListTrigger_String), IntegrationTestHelpers.PollingIntervalShort)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
