using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class SimpleStreamTrigger
    {
        [FunctionName(nameof(SimpleStreamTrigger))]
        public static void Run(
            [RedisStreamTrigger(Common.connectionStringSetting, "streamKey")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}
