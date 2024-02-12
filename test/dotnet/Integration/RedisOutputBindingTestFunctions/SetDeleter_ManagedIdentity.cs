using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SetDeleter_ManagedIdentity
    {
        [FunctionName(nameof(SetDeleter_ManagedIdentity))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ManagedIdentity, IntegrationTestHelpers.KeyeventChannelSet)] ChannelMessage channelMessage,
            [Redis(IntegrationTestHelpers.ManagedIdentity, "DEL")] out string arguments,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{channelMessage.Message}'");
            arguments = channelMessage.Message;
        }
    }
}
