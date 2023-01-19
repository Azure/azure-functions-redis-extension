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
    public class RedisListsTriggerTests
    {
        [Theory]
        [InlineData(nameof(IntegrationTestFunctions.ListsTrigger_SingleKey), IntegrationTestFunctions.listSingleKey, "a b")]
        //[InlineData(nameof(IntegrationTestFunctions.ListsTrigger_MultipleKeys), IntegrationTestFunctions.listMultipleKeys, "a b c d e f")] //fails on anythign before redis7, test is redis6
        public async void ListsTrigger_SuccessfullyTriggers(string functionName, string keys, string values)
        {
            bool success = false;
            string[] keyArray = keys.Split(' ');
            RedisValue[] valuesArray = values.Split(' ').Select((value) => new RedisValue(value)).ToArray();

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", keyArray.Length * valuesArray.Length },
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7074))
            {
                TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);

                foreach (string key in keyArray)
                {
                    await multiplexer.GetDatabase().ListLeftPushAsync(key, valuesArray);
                }

                success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(5));

                multiplexer.Close();
                functionsProcess.Kill();
            };
            Assert.True(success, JsonSerializer.Serialize(counts));
        }
    }
}
