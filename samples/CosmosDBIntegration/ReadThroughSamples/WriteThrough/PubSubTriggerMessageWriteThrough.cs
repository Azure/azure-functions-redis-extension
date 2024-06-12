using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteThrough
{
    public static class PubSubTriggerMessageWriteThrough
    {
        //Connection string settings that will be resolved from local.settings.json file
        public const string RedisConnectionSetting = "RedisConnectionString";
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%PubSubCosmosDbContainerId%";
        public const string PubSubContainerSetting = "%MessagesCosmosDbContainerId%";
        public const string PubSubChannelSetting = "%PubSubChannel%";

        private static readonly IDatabase s_redisDb =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionSetting)).GetDatabase();

        /// <summary>
        /// Triggers when Redis pubsub messages are sent and synchronously writes the message and channel to Cosmos DB.
        /// </summary>
        /// <param name="pubSubMessage"> The message that was published in Redis</param>
        /// <param name="cosmosDBOut"> A dynamic object that is used to synchronously write new data to CosmosDB.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        [FunctionName(nameof(PubSubTriggerMessageWriteThrough))]
        public static void Run(
            [RedisPubSubTrigger(RedisConnectionSetting, PubSubChannelSetting)] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: DatabaseSetting,
                containerName: PubSubContainerSetting,
                Connection = CosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //assign the message from Redis to a dynamic object that will be written to Cosmos DB
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{Environment.GetEnvironmentVariable(PubSubContainerSetting.Replace("%", ""))}\" with id: \"{cosmosDBOut.id}\"");
        }
    }
}
