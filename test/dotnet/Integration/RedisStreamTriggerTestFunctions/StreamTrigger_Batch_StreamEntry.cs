using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Batch_StreamEntry
    {
        [FunctionName(nameof(StreamTrigger_Batch_StreamEntry))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(StreamTrigger_Batch_StreamEntry), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] StreamEntry[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
