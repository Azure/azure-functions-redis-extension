using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;
using System.Net.Http;
using System.Text;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisIntegrationTests")]
    public class RedisStreamTriggerTests
    {
        [Fact]
        public async void StreamTrigger_SuccessfullyTriggers()
        {
            string functionName = nameof(RedisStreamTriggerTestFunctions.StreamTrigger_StreamEntry);
            string[] namesArray = new string[] { "a", "c" };
            string[] valuesArray = new string[] { "b", "d" };

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
            };

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                redis.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Fact]
        public async void StreamTrigger_ScaledOutInstances_DoesntDuplicateEvents()
        {
            string functionName = nameof(RedisStreamTriggerTestFunctions.StreamTrigger_StreamEntry);
            int count = 100;
            string[] namesArray = new string[] { "a", "c" };
            string[] valuesArray = new string[] { "b", "d" };

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", count);

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName))
            using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName))
            using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName))
            {
                functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                for (int i = 0; i < count; i++)
                {
                    await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);
                }

                await Task.Delay(TimeSpan.FromSeconds(count / 4));

                await multiplexer.CloseAsync();
                functionsProcess1.Kill();
                functionsProcess2.Kill();
                functionsProcess3.Kill();
                redis.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_StreamEntry), typeof(StreamEntry))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_NameValueEntryArray), typeof(NameValueEntry[]))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_ByteArray), typeof(byte[]))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_String), typeof(string))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_CustomType), typeof(CustomType))]
        public async void StreamTrigger_TypeConversions_WorkCorrectly(string functionName, Type destinationType)
        {
            string[] namesArray = new string[] { nameof(CustomType.Name), nameof(CustomType.Field)};
            string[] valuesArray = new string[] { "randomName" , "someField" };

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

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                redis.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_Batch_StreamEntry), typeof(StreamEntry[]))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_Batch_NameValueEntryArray), typeof(NameValueEntry[][]))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_Batch_ByteArray), typeof(byte[][]))]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_Batch_String), typeof(string[]))]
        public async void StreamTrigger_Batch_ExecutesFewerTimes(string functionName, Type destinationType)
        {
            int elements = 1000;
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded",  elements / RedisStreamTriggerTestFunctions.batchSize},
                { destinationType.FullName, elements / RedisStreamTriggerTestFunctions.batchSize},
            };

            using (Process redis = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);

                foreach (int i in Enumerable.Range(0, elements))
                {
                    await multiplexer.GetDatabase().StreamAddAsync(functionName, i, i);
                }

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await Task.Delay(elements / RedisStreamTriggerTestFunctions.batchSize * RedisStreamTriggerTestFunctions.pollingIntervalShort * 2);

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
                redis.Kill();
            }
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }
        
        //[Fact]
        //public async void StreamTrigger_TargetBasedScaling_E2EValidation()
        //{
        //    string functionName = nameof(RedisStreamTriggerTestFunctions.StreamTrigger_RedisValue_LongPollingInterval);
        //    int port = 7071;
        //    int elements = 10000;

        //    using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.connectionStringSetting)))
        //    {
        //        await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
        //        for(int i = 0; i < elements; i++)
        //        {
        //            await multiplexer.GetDatabase().StreamAddAsync(functionName, i, i);
        //        }
        //        await multiplexer.CloseAsync();
        //    };

        //    IntegrationTestHelpers.ScaleStatus status = new IntegrationTestHelpers.ScaleStatus { vote = 0, targetWorkerCount = 0 };
        //    using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
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
