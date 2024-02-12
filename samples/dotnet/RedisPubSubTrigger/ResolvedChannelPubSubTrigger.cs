using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class ResolvedChannelPubSubTrigger
    {
        [FunctionName(nameof(ResolvedChannelPubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "%pubsubChannel%")] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation(message.Message);
        }
    }
}
