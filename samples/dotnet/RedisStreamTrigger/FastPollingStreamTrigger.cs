﻿using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisStreamTrigger
{
    internal class FastPollingStreamTrigger
    {
        [FunctionName(nameof(FastPollingStreamTrigger))]
        public static void Run(
            [RedisStreamTrigger(Common.connectionString, "streamKey", pollingIntervalInMs: 100)] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}
