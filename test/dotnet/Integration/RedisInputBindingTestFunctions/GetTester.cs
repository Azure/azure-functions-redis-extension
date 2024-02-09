using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class GetTester
    {
        [FunctionName(nameof(GetTester))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, "__keyevent@0__:set")] ChannelMessage message,
            [Redis(IntegrationTestHelpers.ConnectionStringSetting, "GET {Message}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{message.Message}' was set to value '{value}'");
        }
    }
}
