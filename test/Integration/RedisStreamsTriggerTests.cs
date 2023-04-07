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
    [Collection("RedisTriggerTests")]
    public class RedisStreamsTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_KeyValuePair_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_IReadOnlyDictionary_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_KeyValuePair_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        [InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_IReadOnlyDictionary_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        public async void StreamsTrigger_SuccessfullyTriggers(string functionName, string keys, string names, string values, int entries)
        {
            string[] keyArray = keys.Split(' ');
            string[] namesArray = names.Split(' ');
            string[] valuesArray = values.Split(' ');
            int streamEntryIdStart = 1000;
            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            KeyValuePair<string, string>[] pairs = new KeyValuePair<string, string>[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
                pairs[i] = new KeyValuePair<string, string>(namesArray[i], valuesArray[i]);
            }

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length * entries);

            if (functionName.Contains(nameof(RedisStreamEntry)))
            {
                foreach (string key in keyArray)
                {
                    for (int i = 0; i < entries; i++)
                    {
                        counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisStreamEntry(key, $"{streamEntryIdStart + i}-0", pairs))), 1, (s, c) => c + 1);
                    }
                }
            }
            else if (functionName.Contains(nameof(KeyValuePair)))
            {
                counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(pairs)), keyArray.Length * entries, (s, c) => c + keyArray.Length * entries);
            }
            else if (functionName.Contains(nameof(IReadOnlyDictionary<string, string>)))
            {
                counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(pairs.ToDictionary())), keyArray.Length * entries, (s, c) => c + keyArray.Length * entries);
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamsTriggerTestFunctions.localhostSetting)))
            {
                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().KeyDeleteAsync(key);
                }

                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                    foreach (string key in keyArray)
                    {
                        for (int i = 0; i < entries; i++)
                        {
                            await multiplexer.GetDatabase().StreamAddAsync(key, nameValueEntries, streamEntryIdStart + i);
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(keyArray.Length * entries));

                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
            }
        }

        //[Theory]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_KeyValuePair_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_IReadOnlyDictionary_SingleKey), RedisStreamsTriggerTestFunctions.streamSingleKey, "a c", "b d", 6)]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_RedisStreamEntry_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_KeyValuePair_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        //[InlineData(nameof(RedisStreamsTriggerTestFunctions.StreamsTrigger_IReadOnlyDictionary_MultipleKeys), RedisStreamsTriggerTestFunctions.streamMultipleKeys, "a c e", "b d f", 6)]
        //public async void StreamsTrigger_ScaledOutInstances_DoesntDuplicateEvents(string functionName, string keys, string names, string values, int entries)
        //{

        //    string[] keyArray = keys.Split(' ');
        //    string[] namesArray = names.Split(' ');
        //    string[] valuesArray = values.Split(' ');
        //    int streamEntryIdStart = 1000;
        //    NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
        //    KeyValuePair<string, string>[] pairs = new KeyValuePair<string, string>[namesArray.Length];
        //    for (int i = 0; i < namesArray.Length; i++)
        //    {
        //        nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
        //        pairs[i] = new KeyValuePair<string, string>(namesArray[i], valuesArray[i]);
        //    }

        //    ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
        //    counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length * entries);

        //    if (functionName.Contains(nameof(RedisStreamEntry)))
        //    {
        //        foreach (string key in keyArray)
        //        {
        //            for (int i = 0; i < entries; i++)
        //            {
        //                counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisStreamEntry(key, $"{streamEntryIdStart + i}-0", pairs))), 1, (s, c) => c + 1);
        //            }
        //        }
        //    }
        //    else if (functionName.Contains(nameof(KeyValuePair)))
        //    {
        //        counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(pairs)), keyArray.Length * entries, (s, c) => c + keyArray.Length * entries);
        //    }
        //    else if (functionName.Contains(nameof(IReadOnlyDictionary<string, string>)))
        //    {
        //        counts.AddOrUpdate(string.Format(RedisStreamsTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(pairs.ToDictionary())), keyArray.Length * entries, (s, c) => c + keyArray.Length * entries);
        //    }

        //    using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisStreamsTriggerTestFunctions.localhostSetting)))
        //    {
        //        foreach (string key in keyArray)
        //        {
        //            await multiplexer.GetDatabase().KeyDeleteAsync(key);
        //        }
        //        using (Process functionsProcess1 = IntegrationTestHelpers.StartFunction(functionName, 7071))
        //        using (Process functionsProcess2 = IntegrationTestHelpers.StartFunction(functionName, 7072))
        //        using (Process functionsProcess3 = IntegrationTestHelpers.StartFunction(functionName, 7073))
        //        {
        //            functionsProcess1.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
        //            functionsProcess2.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
        //            functionsProcess3.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

        //            foreach (string key in keyArray)
        //            {
        //                for (int i = 0; i < entries; i++)
        //                {
        //                    await multiplexer.GetDatabase().StreamAddAsync(key, nameValueEntries, streamEntryIdStart + i);
        //                }
        //            }
        //            await Task.Delay(TimeSpan.FromSeconds(keyArray.Length * entries));

        //            await multiplexer.CloseAsync();
        //            functionsProcess1.Kill();
        //            functionsProcess2.Kill();
        //            functionsProcess3.Kill();
        //        };
        //    }
        //    var incorrect = counts.Where(pair => pair.Value != 0);
        //    Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        //}
    }
}
