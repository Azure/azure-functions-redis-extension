using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.SamplesIsolated
{
    public class CustomTypeListTrigger
    {
        private readonly ILogger<CustomTypeListTrigger> logger;

        public CustomTypeListTrigger(ILogger<CustomTypeListTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(CustomTypeListTrigger))]
        public void Run(
            [RedisListTrigger(Common.localhostSetting, "listKey")] Common.CustomType entry)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}