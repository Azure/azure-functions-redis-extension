using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class GetTester
    {
        [FunctionName(nameof(GetTester))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(GetTester))] string message,
            [Redis(IntegrationTestHelpers.ConnectionStringSetting, $"GET {nameof(GetTester)}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of key '{nameof(GetTester)}' is currently a type {value.GetType()}: '{value}'");
        }
    }
}
