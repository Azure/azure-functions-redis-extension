using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples;


// Draft
namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class StreamCosmosIntegrationTests
    {
        [Fact]
        public async void WriteThrough_SuccessfullyTriggers()
        {
            string functionName = nameof(RedisToCosmos.WriteThrough);
            string streamName = RedisToCosmos.stream1;
            string[] namesArray = new string[] { "a", "c" };
            string[] valuesArray = new string[] { "b", "d" };

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Succeeded", 1},
            };

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisToCosmos.redisLocalHost)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                await multiplexer.GetDatabase().StreamAddAsync(streamName, nameValueEntries);

                await Task.Delay(TimeSpan.FromSeconds(4));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };
            var incorrect = counts.Where(pair => pair.Value != 0);
            Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
        }

        [Fact]
        public async void SuccesfullyInCosmosDB()
        {
            string functionName = nameof(Samples.RedisToCosmos.WriteThrough);
            string streamName = Samples.RedisToCosmos.stream1;
            string[] namesArray = new string[] { "a", "c" };
            string[] valuesArray = new string[] { "b", "d" };
            string messageId;

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, RedisToCosmos.redisLocalHost)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                messageId = await multiplexer.GetDatabase().StreamAddAsync(streamName, nameValueEntries);
                await Task.Delay(TimeSpan.FromSeconds(4));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };

            Assert.Equal(StreamCosmosIntegrationTestHelpers.getCosmosDBValuesAsync(messageId).Result, messageId + " a c d b");
        }
    }
}