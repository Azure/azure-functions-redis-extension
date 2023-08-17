using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using Xunit;
using System.Text;

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

        //Target Scaler Integration Tests not required.
        // Keeping this as a manual test for local development.
        //[Fact]
        //public async void ListTrigger_TargetBasedScaling_WorksCorrectly()
        //{
        //    string functionName = nameof(RedisListTriggerTestFunctions.ListTrigger_RedisValue_LongPollingInterval);
        //    int port = 7071;
        //    int elements = 10000;
        //    RedisValue[] valuesArray = Enumerable.Range(0, elements).Select(x => new RedisValue(x.ToString())).ToArray();

        //    using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListTriggerTestFunctions.localhostSetting)))
        //    {
        //        await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
        //        await multiplexer.GetDatabase().ListLeftPushAsync(functionName, valuesArray);
        //        await multiplexer.CloseAsync();
        //    };

        //    IntegrationTestHelpers.ScaleStatus status = new IntegrationTestHelpers.ScaleStatus { vote = 0, targetWorkerCount = 0 };
        //    using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, port))
        //    using (HttpClient client = new HttpClient())
        //    using (StringContent jsonContent = new StringContent("{}", Encoding.UTF8, "application/json"))
        //    {
        //        StringContent content = new StringContent(JsonConvert.SerializeObject(new { name = functionName, arguments = new string[] { "1" } }));
        //        HttpResponseMessage response = await client.PostAsync($"http://127.0.0.1:{port}/admin/host/scale/status", jsonContent);
        //        status = JsonConvert.DeserializeObject<IntegrationTestHelpers.ScaleStatus>(await response.Content.ReadAsStringAsync());
        //        functionsProcess.Kill();
        //    };

        //    Assert.Equal(1, status.vote);
        //    Assert.True(status.targetWorkerCount / (float) elements > 0.999);
        //}
    }
}
