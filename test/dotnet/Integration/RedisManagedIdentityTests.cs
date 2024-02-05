using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;
using static Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration.IntegrationTestHelpers;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("RedisTriggerTests")]
    public class RedisManagedIdentityTests
    {
        [Fact]
        public async void SetDeleter_SuccessfullyDeletes()
        {
            string functionName = nameof(SetDeleter_ManagedIdentity);
            ConcurrentDictionary<string, int> counts = new ConcurrentDictionary<string, int>();
            counts.TryAdd($"Executed '{functionName}' (Succeeded", 1);

            bool exists = true;
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(await RedisUtilities.ResolveConfigurationOptionsAsync(IntegrationTestHelpers.localsettings, new DefaultAzureComponentFactory(), IntegrationTestHelpers.ManagedIdentitySetting, "test")))
            {
                using (Process functionsProcess = await IntegrationTestHelpers.StartFunctionAsync(functionName, 7071))
                {
                    functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                    await multiplexer.GetDatabase().StringSetAsync(functionName, "test");

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    exists = await multiplexer.GetDatabase().KeyExistsAsync(functionName);
                    await multiplexer.CloseAsync();
                    functionsProcess.Kill();
                };
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonConvert.SerializeObject(incorrect));
                Assert.False(exists);
            }
        }
    }
}
