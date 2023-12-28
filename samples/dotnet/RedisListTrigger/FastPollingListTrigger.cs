using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisListTrigger
{
    internal class FastPollingListTrigger
    {
        [FunctionName(nameof(FastPollingListTrigger))]
        public static void Run(
            [RedisListTrigger(Common.connectionStringSetting, "listKey", pollingIntervalInMs: 100)] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}
