using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_String
    {
        [FunctionName(nameof(StreamTrigger_String))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionString, nameof(StreamTrigger_String), IntegrationTestHelpers.PollingIntervalShort)] string values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
