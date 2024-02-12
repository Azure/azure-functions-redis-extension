using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class KeyspaceTrigger
    {
        private readonly ILogger<KeyspaceTrigger> logger;

        public KeyspaceTrigger(ILogger<KeyspaceTrigger> logger)
        {
            this.logger = logger;
        }
        
        [Function(nameof(KeyspaceTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyspace@0__:keyspaceTest")] Common.ChannelMessage channelMessage)
        {
            logger.LogInformation($"Key 'keyspaceTest' was updated with operation '{channelMessage.Message}'");
        }
    }
}
