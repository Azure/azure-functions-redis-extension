using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisInputBinding
{
    internal class GetCustomType
    {
        [FunctionName(nameof(GetCustomType))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "__keyevent@0__:set")] string key,
            [Redis(Common.connectionString, "GET {Message}")] Common.CustomType value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{key}' was set to value '{JsonConvert.SerializeObject(value)}'");
        }
    }
}
