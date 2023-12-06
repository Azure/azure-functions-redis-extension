using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class CustomTypeStreamTrigger
    {
        private readonly ILogger<CustomTypeStreamTrigger> logger;

        public CustomTypeStreamTrigger(ILogger<CustomTypeStreamTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(CustomTypeStreamTrigger))]
        public void Run(
            [RedisStreamTrigger(Common.localhostSetting, "streamKey")] Common.CustomType entry)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}
