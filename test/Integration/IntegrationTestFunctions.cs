using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class IntegrationTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string format = "triggerValue:{0}";
        public const string listKey = "listKey";
        public const int pollingInterval = 100;
        public const int count = 100;

        [FunctionName(nameof(ListTrigger))]
        public static void ListTrigger(
            [RedisListTrigger(localhostSetting, listKey, pollingIntervalInMs: pollingInterval)] string entry,
            ILogger logger)
        {
            logger.LogInformation(string.Format(format, entry));
        }
    }
}
