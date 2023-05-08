using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public class PubSubTriggerChannelPattern
    {
        private readonly ILogger<PubSubTriggerChannelPattern> logger;

        public PubSubTriggerChannelPattern(ILogger<PubSubTriggerChannelPattern> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(PubSubTriggerChannelPattern))]
        public void Run(
            [RedisPubSubTrigger("redisLocalhost", "pubsub*")] string message)
        {
            logger.LogInformation(message);
        }
    }
}