using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_RightDirection
    {
        [FunctionName(nameof(ListTrigger_RightDirection))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionString, nameof(ListTrigger_RightDirection), IntegrationTestHelpers.PollingIntervalLong, listDirection: ListDirection.RIGHT)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
