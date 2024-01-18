using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_Batch_RedisValue
    {
        [FunctionName(nameof(ListTrigger_Batch_RedisValue))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_Batch_RedisValue), IntegrationTestHelpers.pollingIntervalShort, IntegrationTestHelpers.batchSize)] RedisValue[] entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
