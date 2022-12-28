using Microsoft.Extensions.Logging;
using StackExchange.Redis;
<<<<<<< HEAD
=======
using System.Collections.Generic;
using System.Linq;
>>>>>>> 770767b (move all existing trigger classes under PubSubTrigger folder, and create completely new StreamsTrigger)
using System.Text.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhost = "127.0.0.1:6379";

        [FunctionName("PubSubTrigger")]
        public static void PubSubTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.PubSub, Trigger = "maran")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyspaceTrigger")]
        public static void KeyspaceTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeySpace, Trigger = "maran2")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyeventTrigger")]
        public static void KeyeventTrigger(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeyEvent, Trigger = "del")] RedisMessageModel result,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
        }

        [FunctionName("KeyeventTriggerConnectionBinding")]
        public static void KeyeventTriggerConnectionBinding(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeyEvent, Trigger = "set")] RedisMessageModel result,
            [RedisConnection(ConnectionString = localhost)] IConnectionMultiplexer connectionMultiplexer,
            ILogger logger)
        {
            logger.LogInformation(JsonSerializer.Serialize(result));
            logger.LogInformation($"Triggered on {result.Trigger} for key {result.Message}");
            logger.LogInformation($"Value of key {result.Message} = {connectionMultiplexer.GetDatabase().StringGet(result.Message)}");
            connectionMultiplexer.Close();
            connectionMultiplexer.Dispose();
        }

        [FunctionName("KeyspaceTriggerCommandBinding")]
        public static void KeyspaceTriggerCommandBinding(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeySpace, Trigger = "keytest")] RedisMessageModel result,
            [RedisCommand(ConnectionString = localhost, RedisCommand = "get", Arguments = "keytest")] RedisResult value,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {result.Message} event for key {result.Trigger}");
            logger.LogInformation($"Value of key {result.Message} = {value}");
        }

        [FunctionName("KeyspaceTriggerScriptBinding")]
        public static void KeyspaceTriggerScriptBinding(
            [RedisTrigger(ConnectionString = localhost, TriggerType = RedisTriggerType.KeySpace, Trigger = "scriptTest")] RedisMessageModel result,
            [RedisScript(ConnectionString = localhost, LuaScript = "return redis.call('GET', KEYS[1])", Keys = "scriptTest")] RedisResult value,
            ILogger logger)
        {
            logger.LogInformation($"Triggered on {result.Message} event for key {result.Trigger}");
            logger.LogInformation($"Value of key {result.Message} = {value}");
        }


        [FunctionName("StreamsTrigger")]
        public static void StreamsTrigger(
            [RedisStreamsTrigger(ConnectionString = "127.0.0.1:6379", Keys = "streamKey" )]
            RedisStream[] streams, ILogger logger)
        {
            foreach (RedisStream stream in streams)
            {
                foreach (StreamEntry entry in stream.Entries)
                {
                    logger.LogInformation($"Key={stream.Key}, Id={entry.Id}, Values={JsonSerializer.Serialize(entry.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString()))}");
                }
            }
        }
    }
}
