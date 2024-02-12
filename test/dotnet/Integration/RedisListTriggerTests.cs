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
            string functionName = nameof(ListTrigger_Single_String);
            RedisValue[] valuesArray = new RedisValue[] { "a", "b" };

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", valuesArray.Length);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
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
            string functionName = nameof(ListTrigger_Single_String);
            int count = 100;
            RedisValue[] valuesArray = Enumerable.Range(0, count).Select(x => new RedisValue(x.ToString())).ToArray();

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", valuesArray.Length);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
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

        [Fact]
        public async void ListsTrigger_RightDirection_PopsElementsFromRight()
        {
            string functionName = nameof(ListTrigger_RightDirection);
            string leftValue = "1";
            string rightValue = "2";
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd(IntegrationTestHelpers.GetLogValue(rightValue), 1);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, leftValue);
                await multiplexer.GetDatabase().ListRightPushAsync(functionName, rightValue);

                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    await Task.Delay(TimeSpan.FromMilliseconds(IntegrationTestHelpers.PollingIntervalLong / 5));
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
            }
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(ListTrigger_Single_String), typeof(string))]
        [InlineData(nameof(ListTrigger_Single_RedisValue), typeof(RedisValue))]
        [InlineData(nameof(ListTrigger_Single_ByteArray), typeof(byte[]))]
        [InlineData(nameof(ListTrigger_Single_CustomType), typeof(CustomType))]
        public async void ListTrigger_TypeConversions_WorkCorrectly(string functionName, Type destinationType)
        {

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);
            counts.TryAdd(destinationType.FullName, 1);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
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
        [InlineData(nameof(ListTrigger_Batch_String), typeof(string[]))]
        [InlineData(nameof(ListTrigger_Batch_RedisValue), typeof(RedisValue[]))]
        [InlineData(nameof(ListTrigger_Batch_ByteArray), typeof(byte[][]))]
        public async void ListTrigger_Batch_ExecutesFewerTimes(string functionName, Type destinationType)
        {
            int elements = 1000;
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", elements / IntegrationTestHelpers.BatchSize);
            counts.TryAdd(destinationType.FullName, elements / IntegrationTestHelpers.BatchSize);

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis62))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                RedisValue[] values = Enumerable.Range(0, elements).Select(n => new RedisValue(JsonConvert.SerializeObject(new CustomType() { Field = n.ToString(), Name = n.ToString(), Random = n.ToString() }))).ToArray();
                await multiplexer.GetDatabase().ListLeftPushAsync(functionName, values.ToArray());
                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await Task.Delay(TimeSpan.FromMilliseconds(elements / IntegrationTestHelpers.BatchSize * IntegrationTestHelpers.PollingIntervalShort * 2));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
            }
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
    }
}
