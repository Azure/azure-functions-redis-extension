﻿using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisOutputBinding
{
    internal class BatchedCollector
    {
        [FunctionName(nameof(BatchedCollector))]
        public static void Run(
            [RedisPubSubTrigger(Common.localhostSetting, nameof(BatchedCollector))] string entry,
            [Redis(Common.localhostSetting, "SET")] IAsyncCollector<string[]> collector,
            ILogger logger)
        {
            string[] keys = entry.Split(',');
            foreach (string key in keys)
            {
                collector.AddAsync(new string[] { key, nameof(BatchedCollector) }).Wait();
            }
        }
    }
}