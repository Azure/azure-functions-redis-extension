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
    ""name"": ""{nameof(ListTrigger_Batch_String)}"",
    ""type"": ""redisListTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""{nameof(ListTrigger_Batch_String)}"",
    ""connectionStringSetting"": ""{IntegrationTestHelpers.ConnectionStringSetting}"",
    ""key"": ""{nameof(ListTrigger_Batch_String)}"",
    ""maxBatchSize"": ""10"",
}}";

        private const string streamTrigger =
$@"{{
    ""name"": ""{nameof(StreamTrigger_Batch_String)}"",
    ""type"": ""redisStreamTrigger"",
    ""direction"": ""in"",
    ""functionName"": ""{nameof(StreamTrigger_Batch_String)}"",
    ""connectionStringSetting"": ""{IntegrationTestHelpers.ConnectionStringSetting}"",
    ""key"": ""{nameof(StreamTrigger_Batch_String)}"",
    ""maxBatchSize"": ""10"",
}}";

        //[Theory]
        [InlineData(listTrigger, nameof(RedisListTriggerScaleMonitor))]
        [InlineData(streamTrigger, nameof(RedisStreamTriggerScaleMonitor))]
        public void ReturnsCorrectMonitorType(string triggerJson, string expectedMonitorType)
        {
            IServiceProvider serviceProvider = A.Fake<IServiceProvider>();
            A.CallTo(() => serviceProvider.GetService(typeof(IConfiguration))).Returns(IntegrationTestHelpers.localsettings);
            A.CallTo(() => serviceProvider.GetService(typeof(INameResolver))).Returns(A.Fake<INameResolver>());
            A.CallTo(() => serviceProvider.GetService(typeof(AzureComponentFactory))).Returns(A.Fake<AzureComponentFactory>());
            TriggerMetadata metadata = new TriggerMetadata(JObject.Parse(triggerJson));

            string actualMonitorType;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            {
                RedisScalerProvider scalerProvider = new RedisScalerProvider(serviceProvider, metadata);
                actualMonitorType = scalerProvider.GetMonitor().GetType().Name;
                IntegrationTestHelpers.StopRedis(redisProcess);
            }
            Assert.Equal(expectedMonitorType, actualMonitorType);
        }

        //[Theory]
        [InlineData(listTrigger, 0, 0)]
        [InlineData(listTrigger, 1, 1)]
        [InlineData(listTrigger, 100, 10)]
        [InlineData(streamTrigger, 0, 0)]
        [InlineData(streamTrigger, 1, 1)]
        [InlineData(streamTrigger, 100, 10)]
        public async Task ScaleHostEndToEndTest(string triggerJson, int elements, int expectedTarget)
        {
            TriggerMetadata triggerMetadata = new TriggerMetadata(JObject.Parse(triggerJson));
            RedisScalerProvider.RedisPollingTriggerMetadata redisMetadata = JsonConvert.DeserializeObject<RedisScalerProvider.RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());

            AggregateScaleStatus scaleStatus;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionStringSetting, "test")))
            using (IHost scaleHost = await CreateScaleHostAsync(triggerMetadata))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(redisMetadata.key);

                // add some messages
                if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisListTrigger, StringComparison.OrdinalIgnoreCase))
                {
                    RedisValue[] values = Enumerable.Range(0, elements).Select(x => new RedisValue(x.ToString())).ToArray();
                    await multiplexer.GetDatabase().ListLeftPushAsync(redisMetadata.key, values);
                }
                if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisStreamTrigger, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (int value in Enumerable.Range(0, elements))
                    {
                        await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value);
                    }
                }

                IScaleStatusProvider scaleStatusProvider = scaleHost.Services.GetService<IScaleStatusProvider>();
                scaleStatus = await scaleStatusProvider.GetScaleStatusAsync(new ScaleStatusContext());
                await scaleHost.StopAsync();
                IntegrationTestHelpers.StopRedis(redisProcess);
                multiplexer.Close();
            }
            Assert.Equal(expectedTarget, scaleStatus.TargetWorkerCount);
        }

        //[Theory]
        [InlineData(IntegrationTestHelpers.Redis60, 100, 0)]
        [InlineData(IntegrationTestHelpers.Redis60, 0, 100)]
        [InlineData(IntegrationTestHelpers.Redis62, 100, 0)]
        [InlineData(IntegrationTestHelpers.Redis62, 0, 100)]
        [InlineData(IntegrationTestHelpers.Redis70, 100, 0)]
        [InlineData(IntegrationTestHelpers.Redis70, 50, 50)]
        [InlineData(IntegrationTestHelpers.Redis70, 0, 100)]
        public async Task RedisStreamTrigger_SomeElementsProcessed_CalculatesUnprocessedExactly(string redisVersion, int processed, int unprocessed)
        {
            string functionName = nameof(StreamTrigger_Batch_String);
            TriggerMetadata triggerMetadata = new TriggerMetadata(JObject.Parse(streamTrigger));
            RedisScalerProvider.RedisPollingTriggerMetadata redisMetadata = JsonConvert.DeserializeObject<RedisScalerProvider.RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());

            AggregateScaleStatus scaleStatus;
            long streamLength;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(redisVersion))
            using (IHost scaleHost = await CreateScaleHostAsync(triggerMetadata))
            using (ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionStringSetting, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(redisMetadata.key);
                foreach (int value in Enumerable.Range(0, processed))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value);
                }

                Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071);
                await Task.Delay(TimeSpan.FromMilliseconds(2 * processed / IntegrationTestHelpers.BatchSize * IntegrationTestHelpers.PollingIntervalShort));
                functionsProcess.Kill();

                foreach (int value in Enumerable.Range(processed, unprocessed))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value);
                }
                streamLength = multiplexer.GetDatabase().StreamLength(redisMetadata.key);
                multiplexer.Close();

                IScaleStatusProvider scaleStatusProvider = scaleHost.Services.GetService<IScaleStatusProvider>();
                scaleStatus = await scaleStatusProvider.GetScaleStatusAsync(new ScaleStatusContext());
                await scaleHost.StopAsync();
                IntegrationTestHelpers.StopRedis(redisProcess);
            }
            Assert.Equal(processed + unprocessed, streamLength);
            Assert.Equal(unprocessed / IntegrationTestHelpers.BatchSize, scaleStatus.TargetWorkerCount);
        }

        //[Theory]
        [InlineData(IntegrationTestHelpers.Redis60, 75, 25)]
        [InlineData(IntegrationTestHelpers.Redis60, 50, 50)]
        [InlineData(IntegrationTestHelpers.Redis60, 25, 75)]
        [InlineData(IntegrationTestHelpers.Redis62, 75, 25)]
        [InlineData(IntegrationTestHelpers.Redis62, 50, 50)]
        [InlineData(IntegrationTestHelpers.Redis62, 25, 75)]
        public async Task RedisStreamTrigger_CustomIdCounter_ReturnsValidScaleStatus(string redisVersion, int processed, int unprocessed)
        {
            string functionName = nameof(StreamTrigger_Batch_String);
            TriggerMetadata triggerMetadata = new TriggerMetadata(JObject.Parse(streamTrigger));
            RedisScalerProvider.RedisPollingTriggerMetadata redisMetadata = JsonConvert.DeserializeObject<RedisScalerProvider.RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());

            AggregateScaleStatus scaleStatus;
            long streamLength;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(redisVersion))
            using (IHost scaleHost = await CreateScaleHostAsync(triggerMetadata))
            using (ConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionStringSetting, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(redisMetadata.key);
                foreach (int value in Enumerable.Range(0, processed))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value, $"1-{value}");
                }

                Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071);
                await Task.Delay(TimeSpan.FromMilliseconds(2 * processed / IntegrationTestHelpers.BatchSize * IntegrationTestHelpers.PollingIntervalShort));
                functionsProcess.Kill();

                foreach (int value in Enumerable.Range(processed, unprocessed))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(redisMetadata.key, value, value, $"1-{2 * value}");
                }
                streamLength = multiplexer.GetDatabase().StreamLength(redisMetadata.key);
                multiplexer.Close();

                IScaleStatusProvider scaleStatusProvider = scaleHost.Services.GetService<IScaleStatusProvider>();
                scaleStatus = await scaleStatusProvider.GetScaleStatusAsync(new ScaleStatusContext());
                await scaleHost.StopAsync();
                IntegrationTestHelpers.StopRedis(redisProcess);
            }
            Assert.Equal(processed + unprocessed, streamLength);
            Assert.True(scaleStatus.TargetWorkerCount > 0);
            Assert.True(scaleStatus.TargetWorkerCount <= streamLength / IntegrationTestHelpers.BatchSize);
        }

        private async Task<IHost> CreateScaleHostAsync(TriggerMetadata triggerMetadata)
        {
            string hostId = "test-host";
            IHostBuilder hostBuilder = new HostBuilder();
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

            IHost scaleHost = hostBuilder.Build();
            await scaleHost.StartAsync();
            return scaleHost;
        }
    }
}
