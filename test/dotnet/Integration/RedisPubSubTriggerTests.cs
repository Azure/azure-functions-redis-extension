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
        [InlineData(nameof(SingleChannel), IntegrationTestHelpers.PubSubChannel, "testValue")]
        [InlineData(nameof(MultipleChannels), IntegrationTestHelpers.PubSubChannel, "testValue")]
        [InlineData(nameof(MultipleChannels), IntegrationTestHelpers.PubSubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(AllChannels), IntegrationTestHelpers.PubSubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(AllChannels), "prefix" + IntegrationTestHelpers.PubSubChannel, "testPrefix")]
        [InlineData(nameof(AllChannels), "separate", "testSeparate")]
        public async void PubSubTrigger_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { message, 1},
            };

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(channel, message);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(SingleKey), IntegrationTestHelpers.KeyspaceChannel)]
        [InlineData(nameof(MultipleKeys), IntegrationTestHelpers.KeyspaceChannel)]
        [InlineData(nameof(MultipleKeys), IntegrationTestHelpers.KeyspaceChannel + "suffix")]
        [InlineData(nameof(AllKeys), IntegrationTestHelpers.KeyspaceChannel)]
        [InlineData(nameof(AllKeys), IntegrationTestHelpers.KeyspaceChannel + "suffix")]
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

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, "test");
                db.KeyDelete(key);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Fact]
        public async void KeyEventTrigger_SingleEvent_SuccessfullyTriggers()
        {
            string key = "keyTrigger";
            string value = "value";
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(SingleEvent)}' (Succeeded", 1},
                { key, 1},
            };

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(SingleEvent), 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Fact]
        public async void KeyEventTrigger_AllEvents_SuccessfullyTriggers()
        {
            string key = "keyTrigger";
            string value = "value";

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(AllEvents)}' (Succeeded", 2},
                { key, 2},
            };

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(AllEvents), 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                db.KeyDelete(key);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Theory]
        [InlineData(nameof(SingleChannel_ChannelMessage), typeof(ChannelMessage))]
        [InlineData(nameof(SingleChannel_RedisValue), typeof(RedisValue))]
        [InlineData(nameof(SingleChannel_String), typeof(string))]
        [InlineData(nameof(SingleChannel_CustomType), typeof(CustomType))]
        public async void PubSubTrigger_TypeConversions_WorkCorrectly(string functionName, Type destinationType)
        {
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { destinationType.FullName, 1},
            };

            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(IntegrationTestHelpers.PubSubChannel, JsonSerializer.Serialize(new CustomType() { Field = "feeld", Name = "naim", Random = "ran" }));
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
                IntegrationTestHelpers.StopRedis(redisProcess);
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }
    }
}
