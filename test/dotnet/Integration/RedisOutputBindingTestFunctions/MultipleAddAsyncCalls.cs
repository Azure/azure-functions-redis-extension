using StackExchange.Redis;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class MultipleAddAsyncCalls
    {
        [FunctionName(nameof(MultipleAddAsyncCalls))]
        public static async Task Run(
            [RedisPubSubTrigger(IntegrationTestHelpers.ConnectionString, nameof(MultipleAddAsyncCalls))] ChannelMessage entry,
            [Redis(IntegrationTestHelpers.ConnectionString, "SET")] IAsyncCollector<string> collector)
        {
            string[] keys = entry.Message.ToString().Split(',');
            foreach (string key in keys)
            {
                await collector.AddAsync($"{key} {nameof(MultipleAddAsyncCalls)}");
            }
        }
    }
}
