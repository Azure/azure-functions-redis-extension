using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_String_ManagedIdentity
    {
        [FunctionName(nameof(StreamTrigger_String_ManagedIdentity))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ManagedIdentity, nameof(StreamTrigger_String_ManagedIdentity), IntegrationTestHelpers.PollingIntervalShort)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
