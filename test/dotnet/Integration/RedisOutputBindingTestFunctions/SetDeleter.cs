using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SetDeleter
    {
        [FunctionName(nameof(SetDeleter))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, IntegrationTestHelpers.KeyeventChannelSet)] string key,
            [Redis(IntegrationTestHelpers.ConnectionStringSetting, "DEL")] out string arguments,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            arguments = key;
        }
    }
}
