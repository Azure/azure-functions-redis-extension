using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class SetGetter
    {
        [FunctionName(nameof(SetGetter))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "__keyevent@0__:set")] ChannelMessage message,
            [Redis(Common.connectionString, "GET {Message}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{message.Message}' was set to value '{value}'");
        }
    }
}
