using System;
using StackExchange.Redis;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisPubSubTriggerTests
    {
        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_SingleChannel), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_MultipleChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel, "test")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), IntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "prefix" + IntegrationTestFunctions.pubsubChannel, "testPrefix")]
        [InlineData(nameof(IntegrationTestFunctions.PubSubTrigger_AllChannels), "separate", "testSeparate")]
        public void PubSubTrigger_SuccessfullyTriggers(string functionName, string channel, string message)
        {
            bool success = false;
            RedisMessageModel expectedReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.PubSub,
                Trigger = channel,
                Message = message
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(channel, message);
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(1));

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.True(success, JsonSerializer.Serialize(counts));
        }

        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_SingleKey), IntegrationTestFunctions.keyspaceKey)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceKey)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_MultipleKeys), IntegrationTestFunctions.keyspaceKey + "suffix")]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceKey)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), IntegrationTestFunctions.keyspaceKey + "suffix")]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), "prefix" + IntegrationTestFunctions.keyspaceKey)]
        [InlineData(nameof(IntegrationTestFunctions.KeySpaceTrigger_AllKeys), "separate")]
        public void KeySpaceTrigger_SuccessfullyTriggers(string functionName, string key)
        {
            bool success = false;
            RedisMessageModel expectedSetReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.KeySpace,
                Trigger = key,
                Message = "set"
            };
            RedisMessageModel expectedDelReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.KeySpace,
                Trigger = key,
                Message = "del"
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, "test");
                db.KeyDelete(key);
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(1));

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.True(success, JsonSerializer.Serialize(counts));
        }

        [Fact]
        public void KeyEventTrigger_SingleEvent_SuccessfullyTriggers()
        {
            bool success = false;
            string key = "key";
            string value = "value";
            RedisMessageModel expectedReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.KeyEvent,
                Trigger = IntegrationTestFunctions.keyeventEvent,
                Message = key
            };
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent)}' (Succeeded", 1},
                { JsonSerializer.Serialize(expectedReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_SingleEvent), 7071))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(1));

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.True(success, JsonSerializer.Serialize(counts));
        }

        [Fact]
        public void KeyEventTrigger_AllEvents_SuccessfullyTriggers()
        {
            bool success = false;
            string key = "key";
            string value = "value";
            RedisMessageModel expectedSetReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.KeyEvent,
                Trigger = "set",
                Message = key
            };
            RedisMessageModel expectedDelReturn = new RedisMessageModel
            {
                TriggerType = RedisTriggerType.KeyEvent,
                Trigger = "del",
                Message = key
            };

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents)}' (Succeeded", 2},
                { JsonSerializer.Serialize(expectedSetReturn), 1},
                { JsonSerializer.Serialize(expectedDelReturn), 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(nameof(IntegrationTestFunctions.KeyEventTrigger_AllEvents), 7071))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                IDatabase db = multiplexer.GetDatabase();

                db.StringSet(key, value);
                db.KeyDelete(key);
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(1));

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.True(success, JsonSerializer.Serialize(counts));
        }
    }
}
