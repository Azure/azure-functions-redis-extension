using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_LongPollingInterval_RedisValue
    {
        [FunctionName(nameof(ListTrigger_LongPollingInterval_RedisValue))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(ListTrigger_LongPollingInterval_RedisValue), IntegrationTestHelpers.PollingIntervalLong, maxBatchSize: 1)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
