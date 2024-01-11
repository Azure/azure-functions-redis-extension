using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class SetDeleter
    {
        [Function(nameof(SetDeleter))]
        [RedisOutput(Common.connectionStringSetting, "DEL")]
        public static string Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:set")] string key,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            return key;
        }
    }
}
