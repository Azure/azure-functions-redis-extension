﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.RedisInputBinding
{
    internal class GetCustomType
    {
        [FunctionName(nameof(GetCustomType))]
        public static void Run(
            [RedisPubSubTrigger(Common.connectionStringSetting, "__keyevent@0__:set")] string key,
            [Redis(Common.connectionStringSetting, "GET {Message}")] Common.CustomType value,
            ILogger logger)
        {
            logger.LogInformation($"Key '{key}' was set to value '{JsonConvert.SerializeObject(value)}'");
        }
    }
}
