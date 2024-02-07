using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class GetTester_Custom
    {
        [FunctionName(nameof(GetTester_Custom))]
        public static void Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionString,nameof(GetTester_Custom))] string message,
            [Redis(IntegrationTestHelpers.ConnectionString, $"GET {nameof(GetTester_Custom)}")] CustomType value,
            ILogger logger)
        {
            logger.LogInformation($"Value of key '{nameof(GetTester_Custom)}' is currently a type {value.GetType()}: '{JsonConvert.SerializeObject(value)}'");
        }
    }
}
