using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class GetTester_ManagedIdentity
    {
        [FunctionName(nameof(GetTester_ManagedIdentity))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ManagedIdentitySetting, nameof(GetTester_ManagedIdentity))] string message,
            [Redis(IntegrationTestHelpers.ManagedIdentitySetting, $"GET {nameof(GetTester_ManagedIdentity)}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of key '{nameof(GetTester_ManagedIdentity)}' is currently a type {value.GetType()}: '{value}'");
        }
    }
}
