using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisInputBindingTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";

        [FunctionName(nameof(GetTester))]
        public static void GetTester(
            [RedisPubSubTrigger(localhostSetting, nameof(GetTester))] string message,
            [Redis(localhostSetting, $"GET {nameof(GetTester)}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of key '{nameof(GetTester)}' is currently a type {value.GetType()}: '{value}'");
        }

        [FunctionName(nameof(HgetTester))]
        public static void HgetTester(
            [RedisPubSubTrigger(localhostSetting, nameof(HgetTester))] string message,
            [Redis(localhostSetting, $"HGET {nameof(HgetTester)} field")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Value of field 'field' in hash '{nameof(HgetTester)}' is currently '{value}'");
        }

        [FunctionName(nameof(GetTester_Custom))]
        public static void GetTester_Custom(
            [RedisPubSubTrigger(localhostSetting,nameof(GetTester_Custom))] string message,
            [Redis(localhostSetting, $"GET {nameof(GetTester_Custom)}")] CustomType value,
            ILogger logger)
        {
            logger.LogInformation($"Value of key '{nameof(GetTester_Custom)}' is currently a type {value.GetType()}: '{JsonConvert.SerializeObject(value)}'");
        }
    }
}
