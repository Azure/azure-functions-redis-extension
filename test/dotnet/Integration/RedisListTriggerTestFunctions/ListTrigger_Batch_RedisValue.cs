using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Batch_RedisValue
    {
        [FunctionName(nameof(ListTrigger_Batch_RedisValue))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.ConnectionString, nameof(ListTrigger_Batch_RedisValue), IntegrationTestHelpers.PollingIntervalShort, IntegrationTestHelpers.BatchSize)] RedisValue[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
