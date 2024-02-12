using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class StreamTrigger_NameValueEntryArray
    {
        [FunctionName(nameof(StreamTrigger_NameValueEntryArray))]
        public static void Run(
            [RedisStreamTrigger(IntegrationTestHelpers.ConnectionString, nameof(StreamTrigger_NameValueEntryArray), IntegrationTestHelpers.PollingIntervalShort)] NameValueEntry[] values,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(values));
        }
    }
}
