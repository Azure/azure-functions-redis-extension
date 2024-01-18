using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListTrigger_RedisValue
    {
        [FunctionName(nameof(ListTrigger_RedisValue))]
        public static void Run(
            [RedisListTrigger(IntegrationTestHelpers.connectionStringSetting, nameof(ListTrigger_RedisValue), IntegrationTestHelpers.pollingIntervalShort)] RedisValue entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
