using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class SetDeleter
    {
        private readonly ILogger<SetDeleter> logger;

        public SetDeleter(ILogger<SetDeleter> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(SetDeleter))]
        [RedisOutput(Common.localhostSetting, "DEL")]
        public string[] Run(
            [RedisPubSubTrigger(Common.localhostSetting, "__keyevent@0__:set")] string key)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            return new string[] { key };
        }
    }
}
