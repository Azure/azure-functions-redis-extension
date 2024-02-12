using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
    public class RedisManagedIdentityTests
    {
        [Fact]
        public async void InputBinding_ManagedIdentity_WorksCorrectly()
        {
            string functionName = nameof(GetTester_ManagedIdentity);
            string value = "value";
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd($"Value of key '{functionName}' is currently a type {value.GetType()}: '{value}'", 1);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, new IntegrationTestHelpers.ClientSecretCredentialComponentFactory(), IntegrationTestHelpers.ManagedIdentity, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().StringSetAsync(functionName, value);

                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071, true))
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
        public async void OutputBinding_ManagedIdentity_WorksCorrectly()
        {
            string functionName = nameof(SetDeleter_ManagedIdentity);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);

            bool exists = true;
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, new IntegrationTestHelpers.ClientSecretCredentialComponentFactory(), IntegrationTestHelpers.ManagedIdentity, "test")))
            {
                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071, true))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().StringSetAsync(functionName, "test");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    exists = await multiplexer.GetDatabase().KeyExistsAsync(functionName);
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
                Assert.False(exists);
            }
        }

        [Fact]
        public async void ListTrigger_ManagedIdentity_WorksCorrectly()
        {
            string functionName = nameof(ListTrigger_Single_String_ManagedIdentity);
            Type destinationType = typeof(string);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd(destinationType.FullName, 1);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, new IntegrationTestHelpers.ClientSecretCredentialComponentFactory(), IntegrationTestHelpers.ManagedIdentity, "test")))
            using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071, true))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, JsonConvert.SerializeObject(new CustomType() { Field = "feeld", Name = "naim", Random = "ran" }));
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Fact]
        public async void StreamTrigger_ManagedIdentity_WorksCorrectly()
        {
            string functionName = nameof(StreamTrigger_String_ManagedIdentity);
            Type destinationType = typeof(string);
            string[] namesArray = new string[] { nameof(CustomType.Name), nameof(CustomType.Field) };
            string[] valuesArray = new string[] { "randomName", "someField" };

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { destinationType.FullName, 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, new IntegrationTestHelpers.ClientSecretCredentialComponentFactory(), IntegrationTestHelpers.ManagedIdentity, "test")))
            using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071, true))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
    }
}
