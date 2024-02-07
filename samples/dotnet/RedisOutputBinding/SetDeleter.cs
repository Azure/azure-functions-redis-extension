using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class SetDeleter
    {
        [FunctionName(nameof(SetDeleter))]
        [return: Redis(Common.connectionString, "DEL")]
        public static string Run(
            [RedisPubSubTrigger(Common.connectionString, "__keyevent@0__:set")] string key,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            return key;
        }
    }
}
