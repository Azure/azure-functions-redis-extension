using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_String
    {
        [FunctionName(nameof(ListTrigger_String))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_String), IntegrationTestHelpers.pollingIntervalShort)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
