using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisInputBinding
{
    public class GetCustomType
    {
        private readonly ILogger<GetCustomType> logger;

        public GetCustomType(ILogger<GetCustomType> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(GetCustomType))]
        public void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "__keyevent@0__:set")] string key,
            [RedisInput(Common.localhostSetting, "GET {Message}")] Common.CustomType value)
        {
            logger.LogInformation($"Key '{key}' was set to value '{JsonConvert.SerializeObject(value)}'");
        }
    }
}