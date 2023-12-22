using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisIntegrationTests")]
    public class RedisInputBindingTests
    {
        [Fact]
        public async void Get_SuccessfullyGets()
        {
            string functionName = nameof(RedisInputBindingTestFunctions.GetTester);
            string value = "value";
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd($"Value of key '{functionName}' is currently a type {value.GetType()}: '{value}'", 1);

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().StringSetAsync(functionName, value);

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };

                redis.Kill();
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
            }
        }

        [Fact]
        public async void Hget_SuccessfullyGets()
        {
            string functionName = nameof(RedisInputBindingTestFunctions.HgetTester);
            string value = "value";
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd($"Value of field 'field' in hash '{functionName}' is currently '{value}'", 1);

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().HashSetAsync(functionName, "field", value);

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };

                redis.Kill();
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
            }
        }

        [Fact]
        public async void GetCustom_SuccessfullyConvertsType()
        {
            string functionName = nameof(RedisInputBindingTestFunctions.GetTester_Custom);
            CustomType value = new CustomType { Field = "a", Name = "b", Random = "c" };
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd($"Value of key '{functionName}' is currently a type {value.GetType()}: '{JsonConvert.SerializeObject(value)}'", 1);

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                await multiplexer.GetDatabase().StringSetAsync(functionName, JsonConvert.SerializeObject(value));
                await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                redis.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
    }
}
