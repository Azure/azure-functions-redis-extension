using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public class PubSubTrigger
    {
        private readonly ILogger<PubSubTrigger> logger;

        public PubSubTrigger(ILogger<PubSubTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(PubSubTrigger))]
        public void Run(
            [RedisPubSubTrigger("redisLocalhost", "pubsubTest")] string message)
        {
            logger.LogInformation(message);
        }
    }
}