using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisInputBinding
{
    internal class GetCustomType
    {
        [FunctionName(nameof(GetCustomType))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:set")] ChannelMessage message,
            [Redis(Common.connectionStringSetting, "GET {Message}")] Common.CustomType value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{message.Message}' was set to value '{JsonConvert.SerializeObject(value)}'");
        }
    }
}
