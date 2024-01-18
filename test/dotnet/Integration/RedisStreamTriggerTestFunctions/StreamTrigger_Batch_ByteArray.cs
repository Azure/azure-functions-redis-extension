using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Batch_ByteArray
    {
        [FunctionName(nameof(StreamTrigger_Batch_ByteArray))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(StreamTrigger_Batch_ByteArray), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] byte[][] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
