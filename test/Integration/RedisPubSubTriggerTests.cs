using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisPubSubTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_SingleChannel), RedisPubSubTriggerTestFunctions.pubsubChannel, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_SingleChannel), RedisPubSubTriggerTestFunctions.pubsubChannel, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, "prefix" + RedisPubSubTriggerTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_AllChannels), RedisPubSubTriggerTestFunctions.all, "prefix" + RedisPubSubTriggerTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, "separate", "testSeparate")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_string_AllChannels), RedisPubSubTriggerTestFunctions.all, "separate", "testSeparate")]

        public async void PubSubTrigger_RedisPubSubMessage_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel, string message)
        {
            RedisPubSubMessage expectedReturn = new RedisPubSubMessage(subscriptionChannel, channel, message);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
            };

            if (functionName.Contains(nameof(RedisPubSubMessage)))
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(subscriptionChannel, channel, message))), 1);
            }
            else
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, message), 1);
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_SingleKey), RedisPubSubTriggerTestFunctions.keyspaceChannel, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_string_SingleKey), RedisPubSubTriggerTestFunctions.keyspaceChannel, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_string_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_string_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_string_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_string_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]

        public async void KeySpaceTrigger_RedisPubSubMessage_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
            };

            if (functionName.Contains(nameof(RedisPubSubMessage)))
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(subscriptionChannel, channel, "set"))), 1);
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(subscriptionChannel, channel, "del"))), 1);
            }
            else
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, "set"), 1);
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, "del"), 1);
            }


            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_SingleEvent))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_string_SingleEvent))]
        public async void KeyEventTrigger_RedisPubSubMessage_SingleEvent_SuccessfullyTriggers(string functionName)
        {
            string key = "key";
            string value = "value";
            RedisPubSubMessage expectedReturn = new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannel, RedisPubSubTriggerTestFunctions.keyeventChannel, key);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
            };

            if (functionName.Contains(nameof(RedisPubSubMessage)))
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannel, RedisPubSubTriggerTestFunctions.keyeventChannel, key))), 1);
            }
            else
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, key), 1);
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
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

        [Theory]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_AllEvents))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_string_AllEvents))]
        public async void KeyEventTrigger_RedisPubSubMessage_AllEvents_SuccessfullyTriggers(string functionName)
        {
            string key = "key";
            string value = "value";

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
            };

            if (functionName.Contains(nameof(RedisPubSubMessage)))
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:set", key))), 1);
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, JsonSerializer.Serialize(new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:del", key))), 1);
            }
            else
            {
                counts.Add(string.Format(RedisPubSubTriggerTestFunctions.stringFormat, key), 2);
            }


            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
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
    }
}
