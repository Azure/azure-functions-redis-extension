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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);

                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().ListLeftPushAsync(functionName, valuesArray);

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);

                using (Process functionsProcess1 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                using (Process functionsProcess2 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7072))
                using (Process functionsProcess3 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7073))
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
                    IntegrationTestHelpers.StopRedis(redisProcess);
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, JsonConvert.SerializeObject(new CustomType() { Field = "feeld", Name = "naim", Random = "ran" }));
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_Batch_String), typeof(string[]))]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_Batch_RedisValue), typeof(RedisValue[]))]
        [InlineData(nameof(RedisListTriggerTestFunctions.ListTrigger_Batch_ByteArray), typeof(byte[][]))]
        public async void ListTrigger_Batch_ExecutesFewerTimes(string functionName, Type destinationType)
        {
            int elements = 1000;
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", elements / RedisListTriggerTestFunctions.batchSize);
            counts.TryAdd(destinationType.FullName, elements / RedisListTriggerTestFunctions.batchSize);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis62))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                RedisValue[] values = Enumerable.Range(0, elements).Select(n => new RedisValue(JsonConvert.SerializeObject(new CustomType() { Field = n.ToString(), Name = n.ToString(), Random = n.ToString() }))).ToArray();
                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, values.ToArray());
                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await Task.Delay(TimeSpan.FromMilliseconds(elements / RedisListTriggerTestFunctions.batchSize * RedisListTriggerTestFunctions.pollingIntervalShort * 2));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
            }
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        //Target Scaler Integration Tests not required.
        // Keeping this as a manual test for local development.
        //[Fact]
        //public async void ListTrigger_TargetBasedScaling_E2EValidation()
        //{
        //    string functionName = nameof(RedisListTriggerTestFunctions.ListTrigger_RedisValue_LongPollingInterval);
        //    int port = 7071;
        //    int elements = 10000;
        //    RedisValue[] valuesArray = Enumerable.Range(0, elements).Select(x => new RedisValue(x.ToString())).ToArray();

        //    using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
        //    {
        //        await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
        //        await multiplexer.GetDatabase().ListLeftPushAsync(functionName, valuesArray);
        //        await multiplexer.CloseAsync();
        //    };

        //    IntegrationTestHelpers.ScaleStatus status = new IntegrationTestHelpers.ScaleStatus { vote = 0, targetWorkerCount = 0 };
        //    using (Process functionsProcess = await IntegrationTestHelpers.StartFunction(functionName, port))
        //    using (HttpClient client = new HttpClient())
        //    using (StringContent jsonContent = new StringContent("{}", Encoding.UTF8, "application/json"))
        //    {
        //        StringContent content = new StringContent(JsonConvert.SerializeObject(new { name = functionName, arguments = new string[] { "1" } }));
        //        HttpResponseMessage response = await client.PostAsync($"http://127.0.0.1:{port}/admin/host/scale/status", jsonContent);
        //        status = JsonConvert.DeserializeObject<IntegrationTestHelpers.ScaleStatus>(await response.Content.ReadAsStringAsync());
        //        functionsProcess.Kill();
        //    };

        //    Assert.Equal(1, status.vote);
        //    Assert.True(status.targetWorkerCount / (float)elements > 0.999);
        //}
    }
}
