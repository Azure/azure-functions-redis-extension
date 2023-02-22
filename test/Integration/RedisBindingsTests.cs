using System;
using StackExchange.Redis;
using System.Diagnostics;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class RedisBindingsTests
    {
        [Fact]
        public async void RedisCommand_ReturnsCorrectValue()
        {
            string bindingValue = null;
            string functionName = nameof(IntegrationTestFunctions.CommandBinding);
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1 },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                multiplexer.GetDatabase().KeyDelete(IntegrationTestFunctions.bindingKey);

                multiplexer.GetSubscriber().Publish(IntegrationTestFunctions.pubsubChannel, "start");
                await Task.Delay(TimeSpan.FromSeconds(1));

                bindingValue = multiplexer.GetDatabase().StringGet(IntegrationTestFunctions.bindingKey);

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.Equal(IntegrationTestFunctions.bindingValue + "1", bindingValue);
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }

        [Fact]
        public async void RedisScript_SetsCorrectValue()
        {
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
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);
                multiplexer.GetDatabase().KeyDelete(IntegrationTestFunctions.bindingKey);

                multiplexer.GetSubscriber().Publish(IntegrationTestFunctions.pubsubChannel, "start");
                await Task.Delay(TimeSpan.FromSeconds(1));

                bindingValue = multiplexer.GetDatabase().StringGet(IntegrationTestFunctions.bindingKey);

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.Equal(IntegrationTestFunctions.bindingValue + "2", bindingValue);
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));
        }
    }
}
