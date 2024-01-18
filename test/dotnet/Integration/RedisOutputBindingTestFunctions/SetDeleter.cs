using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SetDeleter
    {
        [FunctionName(nameof(SetDeleter))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, IntegrationTestHelpers.keyeventChannelSet)] string key,
            [Redis(IntegrationTestHelpers.connectionStringSetting, "DEL")] out string arguments,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            arguments = key;
        }
    }
}
