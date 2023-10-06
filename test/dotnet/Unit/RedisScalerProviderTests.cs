using FakeItEasy;
using Xunit;
using System;
using Microsoft.Azure.WebJobs.Host.Scale;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisScalerProviderTests
    {
        private const string listTrigger =
$@"{{
    ""name"": ""listTest"",
    ""type"": ""redisListTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""listTestName"",
    ""connectionStringSetting"": ""localhost"",
    ""key"": ""listTestKey"",
    ""maxBatchSize"": ""4"",
}}";

        private const string streamTrigger =
$@"{{
    ""name"": ""streamTest"",
    ""type"": ""redisListTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""streamTestName"",
    ""connectionStringSetting"": ""localhost"",
    ""key"": ""streamTestKey"",
    ""maxBatchSize"": ""4"",
}}";

        [Theory]
        [InlineData(listTrigger, nameof(RedisListTriggerScaleMonitor))]
        [InlineData(streamTrigger, nameof(RedisStreamTriggerScaleMonitor))]
        public void ReturnsCorrectMonitorType(string triggerJson, string monitorType)
        {
            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            TriggerMetadata metadata = new TriggerMetadata(JObject.Parse(triggerJson));
            RedisScalerProvider scalerProvider = new RedisScalerProvider(serviceProvider, metadata);
            Assert.Equal(monitorType, scalerProvider.GetMonitor().GetType().Name);
        }

    }
}
