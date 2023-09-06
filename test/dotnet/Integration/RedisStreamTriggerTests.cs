using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
            using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
            using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
            {
                functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                for (int i = 0; i < count; i++)
                {
                    await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);
                }

                await Task.Delay(TimeSpan.FromSeconds(count / 5));

                await multiplexer.CloseAsync();
                functionsProcess1.Kill();
                functionsProcess2.Kill();
                functionsProcess3.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisStreamTriggerTestFunctions.StreamTrigger_StreamEntry), typeof(StreamEntry))]
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
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
