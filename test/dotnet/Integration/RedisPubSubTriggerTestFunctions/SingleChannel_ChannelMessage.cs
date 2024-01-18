using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SingleChannel_ChannelMessage
    {
        [FunctionName(nameof(SingleChannel_ChannelMessage))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, IntegrationTestHelpers.pubsubChannel)] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
