using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class SimpleStreamTrigger
    {
        private readonly ILogger<SimpleStreamTrigger> logger;

        public SimpleStreamTrigger(ILogger<SimpleStreamTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(SimpleStreamTrigger))]
        public void Run(
            [RedisStreamTrigger(Common.connectionStringSetting, "streamKey")] string entry)
        {
            logger.LogInformation(entry);
        }
    }
}
