using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class AllEvents
    {
        [FunctionName(nameof(AllEvents))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, IntegrationTestHelpers.KeyeventChannelAll)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
