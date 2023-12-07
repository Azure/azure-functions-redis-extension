using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class FastPollingStreamTrigger
    {
        private readonly ILogger<FastPollingStreamTrigger> logger;

        public FastPollingStreamTrigger(ILogger<FastPollingStreamTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(FastPollingStreamTrigger))]
        public void Run(
            [RedisStreamTrigger(Common.connectionStringSetting, "streamKey", pollingIntervalInMs: 100)] string entry)
        {
            logger.LogInformation(entry);
        }
    }
}
