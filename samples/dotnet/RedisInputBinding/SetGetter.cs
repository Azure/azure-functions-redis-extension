﻿using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisPubSubTrigger
{
    internal class SetGetter
    {
        [FunctionName(nameof(SetGetter))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionString, "__keyevent@0__:set")] string key,
            [Redis(Common.connectionString, "GET {Message}")] string value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{key}' was set to value '{value}'");
        }
    }
}
