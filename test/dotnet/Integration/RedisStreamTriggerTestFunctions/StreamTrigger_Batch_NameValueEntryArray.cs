using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_Batch_NameValueEntryArray
    {
        [FunctionName(nameof(StreamTrigger_Batch_NameValueEntryArray))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(StreamTrigger_Batch_NameValueEntryArray), IntegrationTestHelpers.pollingIntervalShort, IntegrationTestHelpers.batchSize)] NameValueEntry[][] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
