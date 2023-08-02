using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisStreamTriggerTestFunctions
    {
        public const string redisLocalHost = "redisConnectionString";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(StreamTrigger_StreamEntry))]
        public static void StreamTrigger_StreamEntry(
            [RedisStreamTrigger(redisLocalHost, nameof(StreamTrigger_StreamEntry), pollingIntervalInMs: pollingInterval)] StreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(IntegrationTestHelpers.GetLogValue(entry));
        }
    }
}
