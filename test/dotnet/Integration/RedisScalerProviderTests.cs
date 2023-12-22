using FakeItEasy;
using Xunit;
using System;
using Microsoft.Azure.WebJobs.Host.Scale;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Newtonsoft.Json;
using Microsoft.Extensions.Azure;
using System.Diagnostics;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
    public class RedisScalerProviderTests
    {
        private const string listTrigger =
$@"{{
    ""name"": ""listTest"",
    ""type"": ""redisListTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""listTestName"",
    ""connectionStringSetting"": ""redisConnectionString"",
    ""key"": ""listScaleTestKey"",
    ""maxBatchSize"": ""16"",
}}";

        private const string streamTrigger =
$@"{{
    ""name"": ""streamTest"",
    ""type"": ""redisStreamTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""streamTestName"",
    ""connectionStringSetting"": ""redisConnectionString"",
    ""key"": ""streamScaleTestKey"",
    ""maxBatchSize"": ""16"",
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

            Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60);
            RedisScalerProvider scalerProvider = new RedisScalerProvider(serviceProvider, metadata);
            redis.Kill();
            Assert.Equal(monitorType, scalerProvider.GetMonitor().GetType().Name);
        }

        [Theory]
        [InlineData(listTrigger, 127, 8)]
        [InlineData(streamTrigger, 127, 8)]
        public async Task ScaleHostEndToEndTest(string triggerJson, int elements, int expectedTarget)
        {
            string hostId = "test-host";
            IHostBuilder hostBuilder = new HostBuilder();
            TriggerMetadata triggerMetadata = new TriggerMetadata(JObject.Parse(triggerJson));
            RedisScalerProvider.RedisPollingTriggerMetadata redisMetadata = JsonConvert.DeserializeObject<RedisScalerProvider.RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());
            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, config) =>
            {
                config.AddConfiguration(IntegrationTestHelpers.hostsettings);
                config.AddConfiguration(IntegrationTestHelpers.localsettings);
            })
            .ConfigureServices(services =>
            {
                services.AddAzureClientsCore();
                services.AddAzureStorageScaleServices();
                services.AddSingleton(A.Fake<INameResolver>());
            })
            .ConfigureWebJobsScale((context, builder) =>
            {
                builder.UseHostId(hostId);
                builder.AddRedis();
                builder.AddRedisScaleForTrigger(triggerMetadata);
            },
            scaleOptions =>
            {
                scaleOptions.IsTargetScalingEnabled = true;
                scaleOptions.MetricsPurgeEnabled = false;
                scaleOptions.ScaleMetricsMaxAge = TimeSpan.FromMinutes(4);
                scaleOptions.IsRuntimeScalingEnabled = true;
                scaleOptions.ScaleMetricsSampleInterval = TimeSpan.FromSeconds(1);
            });

            Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60);
            ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, "redisConnectionString"));
            await multiplexer.GetDatabase().KeyDeleteAsync(redisMetadata.key);

            IHost scaleHost = hostBuilder.Build();
            await scaleHost.StartAsync();

            // add some messages
            if (triggerMetadata.Type.Equals("redisListTrigger"))
            {
                RedisValue[] values = Enumerable.Range(0, elements).Select(x => new RedisValue(x.ToString())).ToArray();
                await multiplexer.GetDatabase().ListLeftPushAsync(redisMetadata.key, values);
            }
            if (triggerMetadata.Type.Equals("redisStreamTrigger"))
            {
                foreach (int value in Enumerable.Range(0, elements))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value);
                }
            }

            IScaleStatusProvider scaleStatusProvider = scaleHost.Services.GetService<IScaleStatusProvider>();
            AggregateScaleStatus scaleStatus = await scaleStatusProvider.GetScaleStatusAsync(new ScaleStatusContext());

            await scaleHost.StopAsync();
            redis.Kill();
            Assert.Equal(ScaleVote.ScaleOut, scaleStatus.Vote);
            Assert.Equal(expectedTarget, scaleStatus.TargetWorkerCount);
        }
    }
}
