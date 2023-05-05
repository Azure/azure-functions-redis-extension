using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Tests
{
    public class PubSubTriggerResolvedChannel
    {
        private readonly ILogger<PubSubTriggerResolvedChannel> logger;

        public PubSubTriggerResolvedChannel(ILogger<PubSubTriggerResolvedChannel> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(PubSubTriggerResolvedChannel))]
        public void Run(
            [RedisPubSubTrigger("redisLocalhost", "%pubsubChannel%")] string message)
        {
            logger.LogInformation(message);
        }
    }
}