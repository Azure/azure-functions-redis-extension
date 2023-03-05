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
    public class RedisTriggerTests
    {
        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_SingleChannel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "prefix" + IntegrationTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "separate", "testSeparate")]
        public async void PubSubTrigger_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            RedisMessageModel expectedReturn = new RedisMessageModel
            {
                Trigger = channel,
                Message = message
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(channel, message);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_SingleKey), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_SuccessfullyTriggers(string functionName, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);
            RedisMessageModel expectedSetReturn = new RedisMessageModel
            {
                Trigger = channel,
                Message = "set"
            };
            RedisMessageModel expectedDelReturn = new RedisMessageModel
            {
                Trigger = channel,
                Message = "del"
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, "test");
                db.KeyDelete(key);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Fact]
        public async void KeyEventTrigger_SingleEvent_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";
            RedisMessageModel expectedReturn = new RedisMessageModel
            {
                Trigger = IntegrationTestFunctions.keyeventChannel,
                Message = key
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent)}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent), 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Fact]
        public async void KeyEventTrigger_AllEvents_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";
            RedisMessageModel expectedSetReturn = new RedisMessageModel
            {
                Trigger = "__keyevent@0__:set",
                Message = key
            };
            RedisMessageModel expectedDelReturn = new RedisMessageModel
            {
                Trigger = "__keyevent@0__:del",
                Message = key
            };

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents)}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents), 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                db.KeyDelete(key);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anythign before redis7, test is redis6
        public async void ListsTrigger_SuccessfullyTriggers(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            RedisValue[] valuesArray = values.Split(' ').Select((value) => new RedisValue(value)).ToArray();

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
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
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anythign before redis7, test is redis6
        public async void ListsTrigger_ScaledOutInstances_DoesntDuplicateEvents(string functionName, string keys, string values)
        {
            string[] keyArray = keys.Split(' ');
            RedisValue[] valuesArray = values.Split(' ').Select((value) => new RedisValue(value)).ToArray();

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length);

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

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_SingleKey), IntegrationTestFunctions.streamSingleKey, "a c", "b d")]
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_MultipleKeys), IntegrationTestFunctions.streamMultipleKeys, "a c e", "b d f")]
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

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
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
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_SingleKey), IntegrationTestFunctions.streamSingleKey, "a c", "b d")]
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_MultipleKeys), IntegrationTestFunctions.streamMultipleKeys, "a c e", "b d f")]
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
