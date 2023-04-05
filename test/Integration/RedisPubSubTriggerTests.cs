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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_ChannelPattern), RedisPubSubTriggerTestFunctions.pubsubPattern, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, "prefix" + RedisPubSubTriggerTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.PubSubTrigger_RedisPubSubMessage_AllChannels), RedisPubSubTriggerTestFunctions.all, "separate", "testSeparate")]
        public async void PubSubTrigger_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel, string message)
        {
            RedisPubSubMessage expectedReturn = new RedisPubSubMessage(subscriptionChannel, channel, message);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_SingleKey), RedisPubSubTriggerTestFunctions.keyspaceChannel, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_MultipleKeys), RedisPubSubTriggerTestFunctions.keyspacePattern, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.KeySpaceTrigger_RedisPubSubMessage_AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannelAll, RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_SuccessfullyTriggers(string functionName, string subscriptionChannel, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);
            RedisPubSubMessage expectedSetReturn = new RedisPubSubMessage(subscriptionChannel, channel, "set");
            RedisPubSubMessage expectedDelReturn = new RedisPubSubMessage(subscriptionChannel, channel, "del");

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
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
            RedisPubSubMessage expectedReturn = new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannel, RedisPubSubTriggerTestFunctions.keyeventChannel, key);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_SingleEvent)}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_SingleEvent), 7071))
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
            RedisPubSubMessage expectedSetReturn = new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:set", key);
            RedisPubSubMessage expectedDelReturn = new RedisPubSubMessage(RedisPubSubTriggerTestFunctions.keyeventChannelAll, "__keyevent@0__:del", key);

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_AllEvents)}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisPubSubTriggerTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(RedisPubSubTriggerTestFunctions.KeyEventTrigger_RedisPubSubMessage_AllEvents), 7071))
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
