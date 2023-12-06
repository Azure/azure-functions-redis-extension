using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class ReturnOutput_StringArray
    {
        [FunctionName(nameof(ReturnOutput_StringArray))]
        [return: Redis(Common.localhostSetting, "SET")]
        public static string[] Run(
            [RedisPubSubTrigger(Common.localhostSetting, "pubsubTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
            return new string[] { nameof(ReturnOutput_StringArray), message };
        }
    }
}
