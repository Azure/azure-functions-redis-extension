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
    public class RedisTriggerTests
    {
        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_SingleChannel_RedisTriggerModel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels_RedisTriggerModel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels_RedisTriggerModel), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels_RedisTriggerModel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels_RedisTriggerModel), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels_RedisTriggerModel), "prefix" + IntegrationTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels_RedisTriggerModel), "separate", "testSeparate")]
        public async void PubSubTrigger_RedisTriggerModel_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            RedisTriggerModel expectedReturn = new RedisTriggerModel
            {
                Trigger = channel,
                Value = message
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
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_SingleChannel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "prefix" + IntegrationTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "separate", "testSeparate")]
        public async void PubSubTrigger_String_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { string.Format(IntegrationTestFunctions.triggerValueFormat, message), 1},
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
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_SingleKey_RedisTriggerModel), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys_RedisTriggerModel), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys_RedisTriggerModel), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys_RedisTriggerModel), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys_RedisTriggerModel), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_RedisTriggerModel_SuccessfullyTriggers(string functionName, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);
            RedisTriggerModel expectedSetReturn = new RedisTriggerModel
            {
                Trigger = channel,
                Value = "set"
            };
            RedisTriggerModel expectedDelReturn = new RedisTriggerModel
            {
                Trigger = channel,
                Value = "del"
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

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_SingleKey), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceChannel)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_String_SuccessfullyTriggers(string functionName, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { string.Format(IntegrationTestFunctions.triggerValueFormat, "set"), 1},
                { string.Format(IntegrationTestFunctions.triggerValueFormat, "del"), 1},
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
        public async void KeyEventTrigger_RedisMessageModel_SingleEvent_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";
            RedisTriggerModel expectedReturn = new RedisTriggerModel
            {
                Trigger = IntegrationTestFunctions.keyeventChannel,
                Value = key
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent_RedisTriggerModel)}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent_RedisTriggerModel), 7071))
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
        public async void KeyEventTrigger_String_SingleEvent_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent)}' (Succeeded", 1},
                { string.Format(IntegrationTestFunctions.triggerValueFormat, key), 1},
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
        public async void KeyEventTrigger_RedisMessageModel_AllEvents_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";
            RedisTriggerModel expectedSetReturn = new RedisTriggerModel
            {
                Trigger = "__keyevent@0__:set",
                Value = key
            };
            RedisTriggerModel expectedDelReturn = new RedisTriggerModel
            {
                Trigger = "__keyevent@0__:del",
                Value = key
            };

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents_RedisTriggerModel)}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents_RedisTriggerModel), 7071))
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

        [Fact]
        public async void KeyEventTrigger_String_AllEvents_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents)}' (Succeeded", 2},
                { string.Format(IntegrationTestFunctions.triggerValueFormat, key), 2},
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

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_SingleKey_RedisTriggerModel), IntegrationTestFunctions.streamSingleKey, "a c", "b d")]
        [InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_DefaultGroup_MultipleKeys_RedisTriggerModel), IntegrationTestFunctions.streamMultipleKeys, "a c e", "b d f")]
        public async void StreamsTrigger_RedisTriggerModel_SuccessfullyTriggers(string functionName, string keys, string names, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] namesArray = names.Split(' ');
            string[] valuesArray = values.Split(' ');
            Dictionary<string, string> expectedReturn = namesArray.Zip(valuesArray).ToDictionary(v => v.First, v => v.Second);

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length },
            };

            foreach (string key in keyArray)
            {
                string expectedReturnString = JsonSerializer.Serialize(new RedisTriggerModel { Trigger = key, Value = expectedReturn });
                if (counts.ContainsKey(expectedReturnString))
                {
                    counts[expectedReturnString]++;
                }
                else
                {
                    counts.TryAdd(expectedReturnString, 1);
                }
            }

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
        public async void StreamsTrigger_Dictionary_SuccessfullyTriggers(string functionName, string keys, string names, string values)
        {
            string[] keyArray = keys.Split(' ');
            string[] namesArray = names.Split(' ');
            string[] valuesArray = values.Split(' ');
            Dictionary<string, string> expectedReturn = namesArray.Zip(valuesArray).ToDictionary(v => v.First, v => v.Second);

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length },
                { JsonSerializer.Serialize(expectedReturn), keyArray.Length }
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
            Dictionary<string, string> expectedReturn = namesArray.Zip(valuesArray).ToDictionary(v => v.First, v => v.Second);

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", keyArray.Length);
            counts.TryAdd(JsonSerializer.Serialize(expectedReturn), keyArray.Length);

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
