using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisStreamsTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d")]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f")]
        public async void StreamsTrigger_SuccessfullyTriggers(string functionName, string keys, string names, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] namesArray = names.Split(' ');
            string[] valuesArray = values.Split(' ');

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamsTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().StreamAddAsync(key, nameValueEntries);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d")]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f")]
        public async void StreamsTrigger_ScaledOutInstances_DoesntDuplicateEvents(string functionName, string keys, string names, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] namesArray = names.Split(' ');
            string[] valuesArray = values.Split(' ');

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamsTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
            using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
            using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
            {
                functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().StreamAddAsync(key, nameValueEntries);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess1.Kill();
                functionsProcess2.Kill();
                functionsProcess3.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }
    }
}
