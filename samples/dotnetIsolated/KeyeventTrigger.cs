using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Tests
{
    public class KeyeventTrigger
    {
        private readonly ILogger<KeyeventTrigger> logger;

        public KeyeventTrigger(ILogger<KeyeventTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(KeyeventTrigger))]
        public void Run(
            [RedisPubSubTrigger("redisLocalhost", "__keyevent@0__:del")] string message)
        {
            logger.LogInformation(message);
        }
    }
}