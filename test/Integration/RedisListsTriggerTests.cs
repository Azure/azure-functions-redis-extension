﻿using StackExchange.Redis;
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
    public class RedisListsTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_RedisListEntry_SingleKey), RedisListsTriggerTestFunctions.listSingleKey, "a b")]
        [InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_string_SingleKey), RedisListsTriggerTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_RedisListEntry_MultipleKeys), RedisListsTriggerTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6
        //[InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_string_MultipleKeys), RedisListsTriggerTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6
        public async void ListsTrigger_SuccessfullyTriggers(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            RedisValue[] valuesArray = values.Split(' ').Select((value) => new RedisValue(value)).ToArray();

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListsTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_RedisListEntry_SingleKey), RedisListsTriggerTestFunctions.listSingleKey, "a b")]
        [InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_string_SingleKey), RedisListsTriggerTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_RedisListEntry_MultipleKeys), RedisListsTriggerTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6
        //[InlineData(nameof(RedisListsTriggerTestFunctions.ListsTrigger_string_MultipleKeys), RedisListsTriggerTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anything before redis7, test is redis6

        public async void ListsTrigger_ScaledOutInstances_DoesntDuplicateEvents(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            RedisValue[] valuesArray = values.Split(' ').Select((value) => new RedisValue(value)).ToArray();

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length);

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisListsTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
            using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
            using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
            {
                functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray);
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
