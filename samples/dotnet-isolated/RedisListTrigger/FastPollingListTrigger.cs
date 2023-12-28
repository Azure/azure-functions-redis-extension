using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisListTrigger
{
    public class FastPollingListTrigger
    {
        private readonly ILogger<FastPollingListTrigger> logger;

        public FastPollingListTrigger(ILogger<FastPollingListTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(FastPollingListTrigger))]
        public void Run(
            [RedisListTrigger(Common.connectionStringSetting, "listKey", pollingIntervalInMs: 100)] string entry)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}