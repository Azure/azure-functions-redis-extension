using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class HgetTester
    {
        [FunctionName(nameof(HgetTester))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionString, nameof(HgetTester))] string message,
            [Redis(IntegrationTestHelpers.ConnectionString, $"HGET {nameof(HgetTester)} field")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of field 'field' in hash '{nameof(HgetTester)}' is currently '{value}'");
        }
    }
}
