using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SingleChannel
    {
        [FunctionName(nameof(SingleChannel))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, IntegrationTestHelpers.PubSubChannel)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
