using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisListTrigger
{
    internal class SimpleListTrigger
    {
        [FunctionName(nameof(SimpleListTrigger))]
        public static void Run(
            [RedisListTrigger(Common.connectionStringSetting, "listTest")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}
