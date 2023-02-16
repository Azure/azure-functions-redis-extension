using System;
using StackExchange.Redis;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisBindingsTests
    {
        [Fact]
        public void RedisCommand_ReturnsCorrectValue()
        {
            bool success = false;
            string bindingValue = null;
            string functionName = nameof(IntegrationTestFunctions.CommandBinding);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1 },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                multiplexer.GetDatabase().KeyDelete(IntegrationTestFunctions.bindingKey);

                multiplexer.GetSubscriber().Publish(IntegrationTestFunctions.pubsubChannel, "start");
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(5));

                bindingValue = multiplexer.GetDatabase().StringGet(IntegrationTestFunctions.bindingKey);

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.Equal(IntegrationTestFunctions.bindingValue + "1", bindingValue);
            Assert.True(success, JsonSerializer.Serialize(counts));
        }

        [Fact]
        public void RedisScript_ReturnsCorrectValue()
        {
            bool success = false;
            string bindingValue = null;
            string functionName = nameof(IntegrationTestFunctions.ScriptBinding);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1 },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                multiplexer.GetDatabase().KeyDelete(IntegrationTestFunctions.bindingKey);

                multiplexer.GetSubscriber().Publish(IntegrationTestFunctions.pubsubChannel, "start");
                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(5));

                bindingValue = multiplexer.GetDatabase().StringGet(IntegrationTestFunctions.bindingKey);

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.Equal(IntegrationTestFunctions.bindingValue + "2", bindingValue);
            Assert.True(success, JsonSerializer.Serialize(counts));
        }
    }
}
