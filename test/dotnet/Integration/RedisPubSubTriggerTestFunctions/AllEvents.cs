using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class AllEvents
    {
        [FunctionName(nameof(AllEvents))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, IntegrationTestHelpers.keyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
