using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhostSetting = "redisLocalhost";

        [FunctionName(nameof(PubSubTrigger))]
        public static void PubSubTrigger(
            [RedisPubSubTrigger(localhostSetting, "pubsubTest")] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(PubSubTriggerResolvedChannel))]
        public static void PubSubTriggerResolvedChannel(
            [RedisPubSubTrigger(localhostSetting, "%pubsubChannel%")] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeyspaceTrigger))]
        public static void KeyspaceTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyspace@0__:keyspaceTest")] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(KeyeventTrigger))]
        public static void KeyeventTrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:del")] RedisPubSubMessage message,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(message));
        }

        [FunctionName(nameof(ListsTrigger))]
        public static void ListsTrigger(
            [RedisListsTrigger(localhostSetting, "listTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(ListsMultipleTrigger))]
        public static void ListsMultipleTrigger(
            [RedisListsTrigger(localhostSetting, "listTest1 listTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(StreamsTrigger))]
        public static void StreamsTrigger(
            [RedisStreamsTrigger(localhostSetting, "streamTest")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }

        [FunctionName(nameof(StreamsMultipleTriggers))]
        public static void StreamsMultipleTriggers(
            [RedisStreamsTrigger(localhostSetting, "streamTest1 streamTest2")] RedisMessageModel model,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(model));
        }
    }
}
