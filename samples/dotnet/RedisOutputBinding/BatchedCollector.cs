using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class BatchedCollector
    {
        [FunctionName(nameof(BatchedCollector))]
        public static async Task Run(
            [RedisPubSubTrigger(Common.connectionString, nameof(BatchedCollector))] string message,
            [Redis(Common.connectionString, "SET")] IAsyncCollector<string> collector,
            ILogger logger)
        {
            logger.LogInformation(message);
            string[] keys = message.Split(',');
            foreach (string key in keys)
            {
                await collector.AddAsync(key + ' ' + nameof(BatchedCollector));
            }
        }
    }
}
