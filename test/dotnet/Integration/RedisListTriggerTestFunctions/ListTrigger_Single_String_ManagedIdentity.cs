using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Single_String_ManagedIdentity
    {
        [FunctionName(nameof(ListTrigger_Single_String_ManagedIdentity))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ManagedIdentity, nameof(ListTrigger_Single_String_ManagedIdentity), IntegrationTestHelpers.PollingIntervalShort)] string entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
