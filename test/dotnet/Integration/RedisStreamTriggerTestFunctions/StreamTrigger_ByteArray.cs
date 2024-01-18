using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_ByteArray
    {
        [FunctionName(nameof(StreamTrigger_ByteArray))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_ByteArray), IntegrationTestHelpers.pollingIntervalShort)] byte[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
