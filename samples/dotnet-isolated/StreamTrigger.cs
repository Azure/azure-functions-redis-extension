using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public class StreamTrigger
    {
        private readonly ILogger<StreamTrigger> logger;

        public StreamTrigger(ILogger<StreamTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(StreamTrigger))]
        public void Run(
            [RedisStreamTrigger("redisLocalhost", "streamTest")] string entry)
        {
            logger.LogInformation(entry);
        }
    }
}