using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class SingleChannel_CustomType
    {
        [FunctionName(nameof(SingleChannel_CustomType))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, IntegrationTestHelpers.PubSubChannel)] CustomType message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
