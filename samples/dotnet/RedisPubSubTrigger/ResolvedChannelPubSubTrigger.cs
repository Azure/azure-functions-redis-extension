using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class ResolvedChannelPubSubTrigger
    {
        [FunctionName(nameof(ResolvedChannelPubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "%pubsubChannel%")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }
    }
}
