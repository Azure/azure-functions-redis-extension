using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisInputBinding
{
    public class SetGetter
    {
        private readonly ILogger<SetGetter> logger;

        public SetGetter(ILogger<SetGetter> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(SetGetter))]
        public void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "__keyevent@0__:set")] string key,
            [RedisInput(Common.localhostSetting, "GET {Message}")] string value)
        {
            logger.LogInformation($"Key '{key}' was set to value '{value}'");
        }
    }
}