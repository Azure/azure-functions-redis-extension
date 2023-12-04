using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.SamplesIsolated
{
    internal class ResolvedChannelPubSubTrigger
    {
        private readonly ILogger<ResolvedChannelPubSubTrigger> logger;

        public ResolvedChannelPubSubTrigger(ILogger<ResolvedChannelPubSubTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(ResolvedChannelPubSubTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "%pubsubChannel%")] string message)
        {
            logger.LogInformation(message);
        }
    }
}
