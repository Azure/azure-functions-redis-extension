using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.SamplesIsolated
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