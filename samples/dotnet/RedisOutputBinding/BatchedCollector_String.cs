using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class BatchedCollector_String
    {
        [FunctionName(nameof(BatchedCollector_String))]
        public static void Run(
            [RedisPubSubTrigger(Common.localhostSetting, nameof(BatchedCollector_String))] string entry,
            [Redis(Common.localhostSetting, "SET")] IAsyncCollector<string> collector,
            ILogger logger)
        {
            string[] keys = entry.Split(',');
            foreach (string key in keys)
            {
                collector.AddAsync(key + ' ' + nameof(BatchedCollector_String)).Wait();
            }
        }
    }
}
