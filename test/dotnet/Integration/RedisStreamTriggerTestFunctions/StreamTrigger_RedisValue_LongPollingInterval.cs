using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_RedisValue_LongPollingInterval
    {
        [FunctionName(nameof(StreamTrigger_RedisValue_LongPollingInterval))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_RedisValue_LongPollingInterval), IntegrationTestHelpers.pollingIntervalLong, maxBatchSize: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
