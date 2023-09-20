using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class KeyspaceTrigger
    {
        [FunctionName(nameof(KeyspaceTrigger))]
        public static void Run(
            [RedisPubSubTrigger(Common.localhostSetting, "__keyspace@0__:keyspaceTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }
    }
}
