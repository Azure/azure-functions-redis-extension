using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string cosmosDbDatabaseSetting = "%CosmosDatabaseName%";
        public const string cosmosDBContainerSetting = "%CosmosContainerName%";
        public const string cosmosDBPubSubContainerSetting = "%PubSubContainerName%";
        public const string pubSubChannelSetting = "%PubSubChannel%";


        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(localhostSetting)).GetDatabase();


        //write-behind caching: Write to Redis, then write to Cosmos DB asynchronously
        [FunctionName(nameof(WriteBehindAsync))]
        public static async Task WriteBehindAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
            [CosmosDB(
                databaseName: cosmosDbDatabaseSetting,
                containerName: cosmosDBContainerSetting,
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<RedisData> cosmosDBOut,
            ILogger logger)
        {
            //load data from Redis into a record
            RedisData redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: await s_redisDb.StringGetAsync(newKey),
                timestamp: DateTime.UtcNow
                );

            //write the record to Cosmos DB
            await cosmosDBOut.AddAsync(redisData);
            logger.LogInformation($"Key: \"{newKey}\", Value: \"{redisData.value}\" added to Cosmos DB container: \"{cosmosDBContainerSetting}\" at id: \"{redisData.id}\"");
        }

        //Pub/sub Write-Behind: writes pubsub messages from Redis to Cosmos DB
        [FunctionName(nameof(WriteBehindMessageAsync))]
        public static async Task WriteBehindMessageAsync(
            [RedisPubSubTrigger(localhostSetting, pubSubChannelSetting)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: cosmosDbDatabaseSetting,
                containerName: cosmosDBPubSubContainerSetting,
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosDBOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            await cosmosDBOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{redisData.id}\"");
        }
    }
}
