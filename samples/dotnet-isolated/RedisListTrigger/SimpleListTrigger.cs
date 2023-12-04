using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public class SimpleListTrigger
    {
        private readonly ILogger<SimpleListTrigger> logger;

        public SimpleListTrigger(ILogger<SimpleListTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(SimpleListTrigger))]
        public void Run(
            [RedisListTrigger(Common.localhostSetting, "listTest")] string entry)
        {
            logger.LogInformation(entry);
        }
    }
}