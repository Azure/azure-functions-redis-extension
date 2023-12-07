using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisListTrigger
{
    internal class CustomTypeListTrigger
    {
        [FunctionName(nameof(CustomTypeListTrigger))]
        public static void Run(
            [RedisListTrigger(Common.connectionStringSetting, "listKey")] Common.CustomType entry,
            ILogger logger)
        {
            logger.LogInformation(JsonConvert.SerializeObject(entry));
        }
    }
}
