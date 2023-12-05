using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class ReturnOutput
    {
        private readonly ILogger<ReturnOutput> logger;

        public ReturnOutput(ILogger<ReturnOutput> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(ReturnOutput))]
        [RedisOutput(Common.localhostSetting, "SET")]
        public string[] Run(
            [RedisPubSubTrigger(Common.localhostSetting, "pubsubTest")] string message)
        {
            logger.LogInformation(message);
            return new string[] { nameof(ReturnOutput), message };
        }
    }
}
