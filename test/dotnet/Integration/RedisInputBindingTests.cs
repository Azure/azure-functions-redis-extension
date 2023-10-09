﻿using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.appsettingsJson, RedisListTriggerTestFunctions.localhostSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().StringSetAsync(functionName, value);

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.appsettingsJson, RedisListTriggerTestFunctions.localhostSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().HashSetAsync(functionName, "field", value);

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.appsettingsJson, RedisListTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                await multiplexer.GetDatabase().StringSetAsync(functionName, JsonConvert.SerializeObject(value));
                await multiplexer.GetSubscriber().PublishAsync(functionName, "start");

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
    }
}
