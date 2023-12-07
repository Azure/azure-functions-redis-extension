using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class CustomTypeStreamTrigger
    {
        [FunctionName(nameof(CustomTypeStreamTrigger))]
        public static void Run(
            [RedisStreamTrigger(Common.connectionStringSetting, "streamKey")] Common.CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}
