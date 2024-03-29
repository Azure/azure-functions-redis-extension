using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SingleChannel_RedisValue
    {
        [FunctionName(nameof(SingleChannel_RedisValue))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionString, IntegrationTestHelpers.PubSubChannel)] RedisValue message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
