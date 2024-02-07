using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class CustomTypePubSubTrigger
    {
        private readonly ILogger<CustomTypePubSubTrigger> logger;

        public CustomTypePubSubTrigger(ILogger<CustomTypePubSubTrigger> logger)
        {
            this.logger = logger;
        }

        [Function(nameof(CustomTypePubSubTrigger))]
        public void Run(
            [RedisPubSubTrigger(Common.connectionString, "pubsubTest")] Common.CustomType type)
        {
            logger.LogInformation(JsonConvert.SerializeObject(type));
        }
    }
}
