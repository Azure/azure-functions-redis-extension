using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.Json;

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

        [FunctionName(nameof(PubSubTriggerChannelPattern))]
        public static void PubSubTriggerChannelPattern(
            [RedisPubSubTrigger(localhostSetting, "pubsub*")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
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

        [FunctionName(nameof(ListTrigger))]
        public static void ListTrigger(
            [RedisListTrigger(localhostSetting, "listTest")] string entry,
            ILogger logger)
        {
            logger.LogInformation("The value from the list: " + entry);
        }

        [FunctionName(nameof(StreamTrigger))]
        public static void StreamTrigger(
            [RedisStreamTrigger(localhostSetting, "streamTest")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}