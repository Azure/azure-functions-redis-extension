using FakeItEasy;
using Xunit;
using System;
using Microsoft.Azure.WebJobs.Host.Scale;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration;

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
    ""connectionStringSetting"": ""redisLocalhost"",
    ""key"": ""listTestKey"",
    ""maxBatchSize"": ""4"",
}}";

        private const string streamTrigger =
$@"{{
    ""name"": ""streamTest"",
    ""type"": ""redisStreamTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""streamTestName"",
    ""connectionStringSetting"": ""redisLocalhost"",
    ""key"": ""streamTestKey"",
    ""maxBatchSize"": ""8"",
}}";

        [Theory]
        [InlineData(listTrigger, nameof(RedisListTriggerScaleMonitor))]
        [InlineData(streamTrigger, nameof(RedisStreamTriggerScaleMonitor))]
        public void ReturnsCorrectMonitorType(string triggerJson, string monitorType)
        {
            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IConfiguration))).Returns(IntegrationTestHelpers.localsettings);
            A.CallTo(() => serviceProvider.GetService(typeof(INameResolver))).Returns(A.Fake<INameResolver>());
            TriggerMetadata metadata = new TriggerMetadata(JObject.Parse(triggerJson));
            RedisScalerProvider scalerProvider = new RedisScalerProvider(serviceProvider, metadata);
            Assert.Equal(monitorType, scalerProvider.GetMonitor().GetType().Name);
        }
    }
}
