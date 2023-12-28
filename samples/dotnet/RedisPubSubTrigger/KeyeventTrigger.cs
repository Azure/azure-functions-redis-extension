using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class KeyeventTrigger
    {
        [FunctionName(nameof(KeyeventTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:del")] string message,
            ILogger logger)
        {
            logger.LogInformation($"Key '{message}' deleted.");
        }
    }
}
