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
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable($"ConnectionStrings:{redisConnectionSetting}")).GetDatabase();


        //write-through caching: Write to Redis then synchronously write to Cosmos DB
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

            logger.LogInformation($"Key: \"{newKey}\", Value: \"{cosmosDBOut.value}\" addedd to Cosmos DB container: \"{"ContainerId"}\" at id: \"{cosmosDBOut.id}\"");
        }

        //Pub/sub Write-Behind: writes pubsub messages from Redis to Cosmos DB
        [FunctionName(nameof(WriteThroughMessage))]
        public static void WriteThroughMessage(
            [RedisPubSubTrigger(redisConnectionSetting, pubSubChannelSetting)] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: databaseSetting,
                containerName: pubSubContainerSetting,
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{cosmosDBOut.id}\"");
        }
    }
}
