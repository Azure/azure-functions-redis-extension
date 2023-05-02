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
    [Collection("RedisTriggerTests")]
    public class RedisPubSubTriggerTests
    {
        [Theory]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel), RedisPubSubTriggerTestFunctions.pubsubChannel, "testValue")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleChannels), RedisPubSubTriggerTestFunctions.pubsubChannel, "testValue")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleChannels), RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), RedisPubSubTriggerTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), "prefix" + RedisPubSubTriggerTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllChannels), "separate", "testSeparate")]
        public async void PubSubTrigger_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { message, 1},
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleKey), RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleKeys), RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.MultipleKeys), RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannel)]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.AllKeys), RedisPubSubTriggerTestFunctions.keyspaceChannel + "suffix")]
        public async void KeySpaceTrigger_SuccessfullyTriggers(string functionName, string channel)
        {
            string keyspace = "__keyspace@0__:";
            string key = channel.Substring(channel.IndexOf(keyspace) + keyspace.Length);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { "set", 1},
                { "del", 1},
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
                { key, 1},
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
                { key, 2},
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
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel_ChannelMessage), typeof(ChannelMessage))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel_RedisValue), typeof(RedisValue))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel_String), typeof(string))]
        [InlineData(nameof(RedisPubSubTriggerTestFunctions.SingleChannel_CustomType), typeof(CustomType))]
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

                subscriber.Publish(RedisPubSubTriggerTestFunctions.pubsubChannel, JsonSerializer.Serialize(new CustomType() { Field = "feeld", Name = "naim", Random = "ran" }));
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }
    }
}
