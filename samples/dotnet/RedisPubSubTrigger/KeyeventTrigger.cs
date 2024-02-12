using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class KeyeventTrigger
    {
        [FunctionName(nameof(KeyeventTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:del")] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation($"Key '{message.Message}' deleted.");
        }
    }
}
