using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class SimplePubSubTrigger
    {
        private readonly ILogger<SimplePubSubTrigger> logger;

        public SimplePubSubTrigger(ILogger<SimplePubSubTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(SimplePubSubTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "pubsubTest")] Common.ChannelMessage channelMessage)
        {
            logger.LogInformation(channelMessage.Message);
        }
    }
}
