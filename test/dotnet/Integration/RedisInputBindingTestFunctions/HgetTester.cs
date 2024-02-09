using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class HgetTester
    {
        [FunctionName(nameof(HgetTester))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, "__keyevent@0__:hset")] ChannelMessage message,
            [Redis(IntegrationTestHelpers.ConnectionStringSetting, "HGET {Message} field")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of field 'field' in hash at key '{message.Message}' is currently '{value}'");
        }
    }
}
