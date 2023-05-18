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
    [Collection("RedisTriggerTests")]
    public class RedisListTriggerTests
    {
        [Fact]
        public async void ListsTrigger_SuccessfullyTriggers()
        {
            string functionName = nameof(RedisListTriggerTestFunctions.ListTrigger_String);
            RedisValue[] valuesArray = new RedisValue[] { "a", "b" };

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", valuesArray.Length);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListTriggerTestFunctions.localhostSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().ListLeftPushAsync(functionName, valuesArray);

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
            }
        }

        [Fact]
        public async void ListsTrigger_ScaledOutInstances_DoesntDuplicateEvents()
        {
            string functionName = nameof(RedisListTriggerTestFunctions.ListTrigger_String);
            int count = 100;
            RedisValue[] valuesArray = Enumerable.Range(0, count).Select(x => new RedisValue(x.ToString())).ToArray();

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", valuesArray.Length);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListTriggerTestFunctions.localhostSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);

                using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
                using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
                using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
                {
                    functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().ListLeftPushAsync(functionName, valuesArray);

                    await Task.Delay(TimeSpan.FromSeconds(count / 5));

                    await multiplexer.CloseAsync();
                    functionsProcess1.Kill();
                    functionsProcess2.Kill();
                    functionsProcess3.Kill();
                };
            }
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_String), typeof(string))]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_RedisValue), typeof(RedisValue))]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_ByteArray), typeof(byte[]))]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_CustomType), typeof(CustomType))]
        public async void ListTrigger_TypeConversions_WorkCorrectly(string functionName, Type destinationType)
        {

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd(destinationType.FullName, 1);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, JsonConvert.SerializeObject(new CustomType() { Field = "feeld", Name = "naim", Random = "ran" }));
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
    }
}
