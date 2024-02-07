using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Batch_String
    {
        [FunctionName(nameof(StreamTrigger_Batch_String))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionString, nameof(StreamTrigger_Batch_String), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] string[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
