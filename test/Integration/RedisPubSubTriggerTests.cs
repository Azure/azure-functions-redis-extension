using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
    public class RedisPubSubTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel), RedisPubSubTriggerTestFunctions.pubsubChannel, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleChannels), RedisPubSubTriggerTestFunctions.pubsubMultiple, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleChannels), RedisPubSubTriggerTestFunctions.pubsubMultiple, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), RedisPubSubTriggerTestFunctions.allChannels, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), RedisPubSubTriggerTestFunctions.allChannels, "prefix" + RedisPubSubTriggerTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), RedisPubSubTriggerTestFunctions.allChannels, "separate", "testSeparate")]
        public async void PubSubTrigger_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel, string message)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { JsonSerializer.Serialize(new CustomChannelMessage(subscriptionChannel, channel, message)), 1},
            };

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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleKey), RedisPubSubTriggerTestFunctions.keyspaceChannel, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleKeys), RedisPubSubTriggerTestFunctions.keyspaceMultiple, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleKeys), RedisPubSubTriggerTestFunctions.keyspaceMultiple, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { JsonSerializer.Serialize(new CustomChannelMessage(subscriptionChannel, channel, "set")), 1},
                { JsonSerializer.Serialize(new CustomChannelMessage(subscriptionChannel, channel, "del")), 1},
            };

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

        [Fact]
        public async void KeyEventTrigger_SingleEvent_SuccessfullyTriggers()
        {
            string key = "key";
            string value = "value";
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(RedisPubSubTriggerTestFunctions.SingleEvent)}' (Succeeded", 1},
                { JsonSerializer.Serialize(new CustomChannelMessage(RedisPubSubTriggerTestFunctions.keyeventChannelSet, RedisPubSubTriggerTestFunctions.keyeventChannelSet, key)), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(RedisPubSubTriggerTestFunctions.SingleEvent), 7071))
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

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(RedisPubSubTriggerTestFunctions.AllEvents)}' (Succeeded", 2},
                { JsonSerializer.Serialize(new CustomChannelMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:set", key)), 1},
                { JsonSerializer.Serialize(new CustomChannelMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:del", key)), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(RedisPubSubTriggerTestFunctions.AllEvents), 7071))
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.ChannelMessage_SingleChannel), typeof(ChannelMessage))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.RedisValue_SingleChannel), typeof(RedisValue))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.String_SingleChannel), typeof(string))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.CustomChannelMessage_SingleChannel), typeof(CustomChannelMessage))]
        public async void PubSubTrigger_TypeConversions_WorkCorrectly(string functionName, Type destinationType)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { destinationType.FullName, 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(RedisPubSubTriggerTestFunctions.pubsubChannel, "test");
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }
    }
}
