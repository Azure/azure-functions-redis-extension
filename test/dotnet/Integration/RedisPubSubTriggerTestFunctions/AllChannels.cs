using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class AllChannels
    {
        [FunctionName(nameof(AllChannels))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionString, IntegrationTestHelpers.AllChannels)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
