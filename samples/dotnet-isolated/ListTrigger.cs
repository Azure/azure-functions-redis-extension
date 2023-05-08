using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public class ListTrigger
    {
        private readonly ILogger<ListTrigger> logger;

        public ListTrigger(ILogger<ListTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(ListTrigger))]
        public void Run(
            [RedisListTrigger("redisLocalhost", "listTest")] string entry)
        {
            logger.LogInformation(entry);
        }
    }
}