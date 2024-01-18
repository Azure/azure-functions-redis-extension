using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
    public class RedisOutputBindingTests
    {
        [Fact]
        public async void SetDeleter_SuccessfullyDeletes()
        {
            string functionName = nameof(SetDeleter);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);

            bool exists = true;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            {
                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().StringSetAsync(functionName, "test");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    exists = await multiplexer.GetDatabase().KeyExistsAsync(functionName);
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
                Assert.False(exists);
            }
        }

        [Fact]
        public async void StreamTriggerDeleter_SuccessfullyDeletes()
        {
            string functionName = nameof(StreamTriggerDeleter);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);

            string[] namesArray = new string[] { "a", "c" };
            string[] valuesArray = new string[] { "b", "d" };

            NameValueEntry[] nameValueEntries = new NameValueEntry[namesArray.Length];
            for (int i = 0; i < namesArray.Length; i++)
            {
                nameValueEntries[i] = new NameValueEntry(namesArray[i], valuesArray[i]);
            }

            long length = 1;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            {
                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().StreamAddAsync(functionName, nameValueEntries);

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    length = await multiplexer.GetDatabase().StreamLengthAsync(functionName);
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
                Assert.Equal(0, length);
            }
        }

        [Fact]
        public async void MultipleAddAsyncCalls_SuccessfullyFlushes()
        {
            string functionName = nameof(MultipleAddAsyncCalls);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);

            int numKeys = 100;
            string[] keys = Enumerable.Range(0, numKeys).Select(i => i.ToString()).ToArray();
            RedisKey[] rKeys = keys.Select(key => new RedisKey(key)).ToArray();
            string message = string.Join(',', keys);

            long actualKeys = 0;
            using (Process redisProcess = IntegrationTestHelpers.StartRedis(IntegrationTestHelpers.Redis60))
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, IntegrationTestHelpers.ConnectionStringSetting)))
            {
                await multiplexer.GetDatabase().KeyDeleteAsync(rKeys);
                using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetSubscriber().PublishAsync(functionName, message);

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    foreach(string key in keys)
                    {
                        if (await multiplexer.GetDatabase().KeyExistsAsync(key))
                        {
                            actualKeys++;
                        }
                    }
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                    IntegrationTestHelpers.StopRedis(redisProcess);
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
                Assert.Equal(numKeys, actualKeys);
            }
        }
    }
}
