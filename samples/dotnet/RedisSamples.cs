using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhostSetting = "redisLocalhost";

        [FunctionName(nameof(PubSubTrigger))]
        public static void PubSubTrigger(
            [RedisPubSubTrigger(localhostSetting, "pubsubTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(OutputBindingArgumentsOnly))]
        [return: Redis(localhostSetting, "SET")]
        public static string[] OutputBindingArgumentsOnly(
            [RedisPubSubTrigger(localhostSetting, "pubsubTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
            return new string[] { nameof(OutputBindingArgumentsOnly), message };
        }

        [FunctionName(nameof(PubSubTriggerResolvedChannel))]
        public static void PubSubTriggerResolvedChannel(
            [RedisPubSubTrigger(localhostSetting, "%pubsubChannel%")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(KeyspaceTrigger))]
        public static void KeyspaceTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyspace@0__:keyspaceTest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(KeyeventTrigger))]
        public static void KeyeventTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:del")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(ListTriggerSingle))]
        public static void ListTriggerSingle(
            [RedisListTrigger(localhostSetting, "listTest")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }

        [FunctionName(nameof(ListTriggerArray))]
        public static void ListTriggerArray(
            [RedisListTrigger(localhostSetting, "listArrayTest")] string[] entries,
            ILogger logger)
        {
            logger.LogInformation(string.Join(',', entries));
        }

        [FunctionName(nameof(StreamTriggerDeleteEntry))]
        public static void StreamTriggerDeleteEntry(
            [RedisStreamTrigger(localhostSetting, "streamTest2")] StreamEntry entry,
            [Redis(localhostSetting, "XDEL")] out string[] result,
            ILogger logger)
        {
            logger.LogInformation($"Stream entry from key 'streamTest2' with Id '{entry.Id}' and values '" +
                JsonConvert.SerializeObject(entry.Values.ToDictionary(x => x.Name.ToString(), x => x.Value.ToString())));
            result = new string[] { "streamTest2", entry.Id.ToString() };
        }
    }
}
