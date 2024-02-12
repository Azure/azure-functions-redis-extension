using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class SetDeleter
    {
        [FunctionName(nameof(SetDeleter))]
        [return: Redis(Common.connectionStringSetting, "DEL")]
        public static string Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:set")] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{message.Message}'");
            return message.Message;
        }
    }
}
