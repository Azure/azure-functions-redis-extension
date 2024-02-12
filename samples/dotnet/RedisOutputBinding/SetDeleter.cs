using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class SetDeleter
    {
        [FunctionName(nameof(SetDeleter))]
        [return: Redis(Common.connectionString, "DEL")]
        public static string Run(
            [RedisPubSubTrigger(Common.connectionString, "__keyevent@0__:set")] ChannelMessage message,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{message.Message}'");
            return message.Message;
        }
    }
}
