﻿using StackExchange.Redis;
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
    [Collection("RedisTriggerTests")]
    public class RedisStreamTriggerTests
    {
        [Fact]
        public async void StreamTrigger_SuccessfullyTriggers()
        {
            string functionName = nameof(StreamTrigger_StreamEntry);
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);

                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Fact]
        public async void StreamTrigger_ScaledOutInstances_DoesntDuplicateEvents()
        {
            string functionName = nameof(StreamTrigger_StreamEntry);
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            using (Process functionsProcess1 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
            using (Process functionsProcess2 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7072))
            using (Process functionsProcess3 = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7073))
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
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(StreamTrigger_StreamEntry), typeof(StreamEntry))]
        [InlineData(nameof(StreamTrigger_NameValueEntryArray), typeof(NameValueEntry[]))]
        [InlineData(nameof(StreamTrigger_ByteArray), typeof(byte[]))]
        [InlineData(nameof(StreamTrigger_String), typeof(string))]
        [InlineData(nameof(StreamTrigger_CustomType), typeof(CustomType))]
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Theory]
        [InlineData(nameof(StreamTrigger_Batch_StreamEntry), typeof(StreamEntry[]))]
        [InlineData(nameof(StreamTrigger_Batch_NameValueEntryArray), typeof(NameValueEntry[][]))]
        [InlineData(nameof(StreamTrigger_Batch_ByteArray), typeof(byte[][]))]
        [InlineData(nameof(StreamTrigger_Batch_String), typeof(string[]))]
        public async void StreamTrigger_Batch_ExecutesFewerTimes(string functionName, Type destinationType)
        {
            int elements = 1000;
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded",  elements / IntegrationTestHelpers.BatchSize},
                { destinationType.FullName, elements / IntegrationTestHelpers.BatchSize},
            };

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, null, IntegrationTestHelpers.ConnectionString, "test")))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(functionName);
                Task.WaitAll(Enumerable.Range(0, elements).Select(i => multiplexer.GetDatabase().StreamAddAsync(functionName, i, i)).ToArray());

                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await Task.Delay(elements / IntegrationTestHelpers.BatchSize * IntegrationTestHelpers.PollingIntervalShort * 2);

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
