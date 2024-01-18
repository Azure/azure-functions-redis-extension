using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class MultipleKeys
    {
        [FunctionName(nameof(MultipleKeys))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, IntegrationTestHelpers.KeyspaceMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
