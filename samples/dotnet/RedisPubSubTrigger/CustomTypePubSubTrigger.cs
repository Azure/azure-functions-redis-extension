using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class CustomTypePubSubTrigger
    {
        [FunctionName(nameof(CustomTypePubSubTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "pubsubTest")] Common.CustomType type,
            ILogger logger)
        {
            logger.LogInformation(JsonConvert.SerializeObject(type));
        }
    }
}
