using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteThrough
{
    public static class PubSubTriggerWriteThrough
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
        /// Triggers when Redis keys are added or updated and synchronously writes the key/value pair to Cosmos DB.
        /// </summary>
        /// <param name="newKey"> The key that has been added or changed in Redis.</param>
        /// <param name="cosmosDBOut"> A dynamic object that is used to synchronously write new data to CosmosDB.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        [FunctionName(nameof(PubSubTriggerWriteThrough))]
        public static void Run(
           [RedisPubSubTrigger(RedisConnectionSetting, "__keyevent@0__:set")] string newKey,
           [CosmosDB(
                databaseName: DatabaseSetting,
                containerName: ContainerSetting,
                Connection = CosmosDbConnectionSetting)]out dynamic cosmosDBOut,
           ILogger logger)
        {
            //assign the data from Redis to a dynamic object that will be written to Cosmos DB
            cosmosDBOut = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: s_redisDb.StringGet(newKey),
                timestamp: DateTime.UtcNow
            );

            logger.LogInformation($"Key: \"{newKey}\", Value: \"{cosmosDBOut.value}\" addedd to Cosmos DB container: \"{Environment.GetEnvironmentVariable(ContainerSetting.Replace("%",""))}\" at id: \"{cosmosDBOut.id}\"");
        }
    }
}
