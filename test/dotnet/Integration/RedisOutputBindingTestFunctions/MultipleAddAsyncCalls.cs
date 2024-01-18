using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class MultipleAddAsyncCalls
    {
        [FunctionName(nameof(MultipleAddAsyncCalls))]
        public static async Task Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionStringSetting, nameof(MultipleAddAsyncCalls))] string entry,
            [Redis(IntegrationTestHelpers.ConnectionStringSetting, "SET")] IAsyncCollector<string> collector)
        {
            string[] keys = entry.Split(',');
            foreach (string key in keys)
            {
                await collector.AddAsync($"{key} {nameof(MultipleAddAsyncCalls)}");
            }
        }
    }
}
