using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_CustomType
    {
        [FunctionName(nameof(StreamTrigger_CustomType))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(StreamTrigger_CustomType), IntegrationTestHelpers.PollingIntervalShort)] CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
