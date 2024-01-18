using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class MultipleKeys
    {
        [FunctionName(nameof(MultipleKeys))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.connectionStringSetting, IntegrationTestHelpers.keyspaceMultiple)] string message,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(message));
        }
    }
}
