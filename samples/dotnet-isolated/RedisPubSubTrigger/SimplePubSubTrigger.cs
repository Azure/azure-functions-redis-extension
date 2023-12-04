using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.SamplesIsolated
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
            [RedisPubSubTrigger(Common.localhostSetting, "pubsubTest")] string message)
        {
            logger.LogInformation(message);
        }
    }
}
