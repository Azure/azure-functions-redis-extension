using System;
using StackExchange.Redis;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Collections.Concurrent;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisListsTriggerTests
    {
        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey_RedisTriggerModel), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys_RedisTriggerModel), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6
        public async void ListsTrigger_RedisMessageModel_SuccessfullyTriggers(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] valuesArray = values.Split(' ');

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length },
            };

            foreach (string key in keyArray)
            {
                foreach (string value in valuesArray)
                {
                    string expected = JsonSerializer.Serialize(new RedisTriggerModel { Trigger = key, Value = value });
                    if (counts.ContainsKey(expected))
                    {
                        counts[expected]++;
                    }
                    else
                    {
                        counts.Add(expected, 1);
                    }
                }
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray.Select(value => new RedisValue(value)).ToArray());
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6
        public async void ListsTrigger_String_SuccessfullyTriggers(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] valuesArray = values.Split(' ');

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length },
            };

            foreach (string value in valuesArray.Select(value => string.Format(IntegrationTestFunctions.triggerValueFormat, value)))
            {
                if (counts.ContainsKey(value))
                {
                    counts[value] += keyArray.Length;
                }
                else
                {
                    counts.Add(value, keyArray.Length);
                }
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray.Select(value => new RedisValue(value)).ToArray());
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey_RedisTriggerModel), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys_RedisTriggerModel), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anythign before redis7, test is redis6
        public async void ListsTrigger_ScaledOutInstances_DoesntDuplicateEvents(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] valuesArray = values.Split(' ');

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length);

            foreach (string value in valuesArray.Select(value => string.Format(IntegrationTestFunctions.triggerValueFormat, value)))
            {
                if (counts.ContainsKey(value))
                {
                    counts[value] += keyArray.Length;
                }
                else
                {
                    counts.TryAdd(value, keyArray.Length);
                }
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
            using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
            using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
            {
                functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray.Select((value) => new RedisValue(value)).ToArray());
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
