using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_ByteArray
    {
        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionString, nameof(StreamTrigger_ByteArray), IntegrationTestHelpers.PollingIntervalShort)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
