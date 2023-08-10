using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class RedisOutputBindingTestFunctions
    {
        public const string localhostSetting = "redisLocalhost";
        public const string keyeventChannelSet = "__keyevent@0__:set";
        public const int pollingInterval = 100;

        [FunctionName(nameof(SetDeleter))]
        public static void SetDeleter(
            [RedisPubSubTrigger(localhostSetting, keyeventChannelSet)] string key,
            [Redis(localhostSetting, "DEL")] out string[] arguments,
            ILogger logger)
        {
            logger.LogInformation($"Deleting recently SET key '{key}'");
            arguments = new string[] { key };
        }

        [FunctionName(nameof(StreamTriggerDeleter))]
        public static void StreamTriggerDeleter(
            [RedisStreamTrigger(localhostSetting, nameof(StreamTriggerDeleter), pollingIntervalInMs: pollingInterval)] StreamEntry entry,
            [Redis(localhostSetting, "XDEL")] out string[] arguments,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key '{nameof(StreamTriggerDeleter)}' with Id '{entry.Id}' and values '{JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString()))}");
            arguments = new string[] { nameof(StreamTriggerDeleter), entry.Id.ToString() };
        }
    }
}
