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
    public class RedisStreamsTriggerTests
    {
        //[Theory]
        //[InlineData(nameof(IntegrationTestFunctions.StreamsTrigger_WithoutGroup_SingleKey), IntegrationTestFunctions.streamSingleKey, "a c e", "b d f")]
        //public void StreamsTrigger_SuccessfullyTriggers(string functionName, string keys, string names, string values)
        //{
        //    bool success = false;
        //    string[] keyArray = keys.Split(' ');
        //    string[] namesArray = names.Split(' ');
        //    string[] valuesArray = values.Split(' ');
            
        //    NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
        //    for (int i = 0; i < namesArray.Length; i++)
        //    {
        //        nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
        //    }

        //    Dictionary<string, int> counts = new Dictionary<string, int>
        //    {
        //        { $"Executed '{functionName}' (Succeeded", 1},
        //    };

        //    foreach (NameValueEntry entry in nameValueEntries)
        //    {
        //        counts.Add($"{entry.Name},{entry.Value}", 1);
        //    }

        //    using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(IntegrationTestFunctions.connectionString))
        //    using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName))
        //    {
        //        TaskCompletionSource<bool> functionCompleted = new TaskCompletionSource<bool>();
        //        functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts, functionCompleted);
                
        //        foreach (string key in keyArray)
        //        {
        //            multiplexer.GetDatabase().StreamAddAsync(key, nameValueEntries);
        //        }
                
        //        success = functionCompleted.Task.Wait(TimeSpan.FromSeconds(1));

        //        multiplexer.Close();
        //        functionsProcess.Kill();
        //    };
        //    Assert.True(success, JsonSerializer.Serialize(counts));
        //}
    }
}
