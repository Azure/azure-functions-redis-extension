using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class ReturnOutput_String
    {
        [FunctionName(nameof(ReturnOutput_String))]
        [return: Redis(Common.localhostSetting, "SET")]
        public static string Run(
            [RedisPubSubTrigger(Common.localhostSetting, "pubsubTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
            return nameof(ReturnOutput_String) + ' ' + message;
        }
    }
}
