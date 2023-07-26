using Microsoft.Azure.Functions.Worker.Extensions.Redis;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSample
    {
        //Connection string settings that will be resolved from local.settings.json file
        public const string redisConnectionSetting = "RedisConnectionString";
        public const string cosmosDbConnectionSetting = "CosmosDbConnectionString";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string databaseSetting = "%CosmosDbDatabaseId%";
        public const string containerSetting = "%CosmosDbContainerId%";
        public const string pubSubContainerSetting = "%PubSubContainerId%";
        public const string pubSubChannelSetting = "%PubSubChannel%";

        private static readonly IDatabase s_redisDb =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionSetting)).GetDatabase();


        /// <summary>
        /// Triggers when Redis keys are added or updated and synchronously writes the key/value pair to Cosmos DB.
        /// </summary>
        /// <param name="newKey"> The key that has been added or changed in Redis.</param>
        /// <param name="cosmosDBOut"> A dynamic object that is used to synchronously write new data to CosmosDB.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        [FunctionName(nameof(WriteThrough))]
        public static void WriteThrough(
           [RedisPubSubTrigger(redisConnectionSetting, "__keyevent@0__:set")] string newKey,
           [CosmosDB(
                databaseName: databaseSetting,
                containerName: containerSetting,
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
           ILogger logger)
        {
            //assign the data from Redis to a dynamic object that will be written to Cosmos DB
            cosmosDBOut = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: s_redisDb.StringGet(newKey),
                timestamp: DateTime.UtcNow
            );
            
            logger.LogInformation($"Key: \"{newKey}\", Value: \"{cosmosDBOut.value}\" addedd to Cosmos DB container: \"{Environment.GetEnvironmentVariable(containerSetting.Replace("%",""))}\" at id: \"{cosmosDBOut.id}\"");
        }

        /// <summary>
        /// Triggers when Redis pubsub messages are sent and synchronously writes the message and channel to Cosmos DB.
        /// </summary>
        /// <param name="pubSubMessage"> The message that was published in Redis</param>
        /// <param name="cosmosDBOut"> A dynamic object that is used to synchronously write new data to CosmosDB.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        [FunctionName(nameof(WriteThroughMessage))]
        public static void WriteThroughMessage(
            [RedisPubSubTrigger(redisConnectionSetting, pubSubChannelSetting)] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: databaseSetting,
                containerName: pubSubContainerSetting,
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //assign the message from Redis to a dynamic object that will be written to Cosmos DB
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{Environment.GetEnvironmentVariable(pubSubContainerSetting.Replace("%", ""))}\" with id: \"{cosmosDBOut.id}\"");
        }
    }
}
