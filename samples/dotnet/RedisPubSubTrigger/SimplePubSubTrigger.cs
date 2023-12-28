using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class SimplePubSubTrigger
    {
        [FunctionName(nameof(SimplePubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "pubsubTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }
    }
}
