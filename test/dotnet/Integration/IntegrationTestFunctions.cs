using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using StackExchange.Redis;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests
{
    public static class IntegrationTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string format = "triggerValue:{0}";
        public const string listKey = "listKey";
        public const string streamKey = "streamKey";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(ListTrigger))]
        public static void ListTrigger(
            [RedisListTrigger(localhostSetting, listKey, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(format, entry));
        }

        [FunctionName(nameof(StreamsTrigger))]
        public static void StreamsTrigger(
            [RedisStreamTrigger(localhostSetting, streamKey, pollingIntervalInMs: pollingInterval)] RedisStreamEntry entry,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(entry));
        }
    }
}