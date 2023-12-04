using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
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
            [RedisListTrigger(Common.localhostSetting, "listKey", pollingIntervalInMs: 100)] Common.CustomType entry)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}