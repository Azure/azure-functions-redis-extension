using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class ResolvedChannelPubSubTrigger
    {
        [FunctionName(nameof(ResolvedChannelPubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "%pubsubChannel%")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }
    }
}
