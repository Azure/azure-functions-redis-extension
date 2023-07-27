using Microsoft.Azure.Functions.Worker.Extensions.Redis;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSample
    {
        //Connection string settings that will be resolved from local.settings.json file
        public const string redisConnectionSetting = "redisConnectionString";
        public const string cosmosDbConnectionSetting = "CosmosDbConnectionString";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string databaseSetting = "%CosmosDbDatabaseId%";
        public const string containerSetting = "%CosmosDbContainerId%";
        public const string pubSubContainerSetting = "%PubSubContainerId%";
        public const string pubSubChannelSetting = "%PubSubChannel%";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionSetting)).GetDatabase();

 
         /// <summary>
         /// Triggers when Redis keys are added or updated and asynchronously writes the key/value pair to Cosmos DB.
         /// </summary>
         /// <param name="newKey"> The key that has been added or changed in Redis.</param>
         /// <param name="cosmosDBOut"> An IAsyncCollector that is used to write RedisData to Cosmos DB.</param>
         /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
         /// <returns></returns>
        [FunctionName(nameof(WriteBehindAsync))]
        public static async Task WriteBehindAsync(
            [RedisPubSubTrigger(redisConnectionSetting, "__keyevent@0__:set")] string newKey,
            [CosmosDB(
                databaseName: databaseSetting,
                containerName: containerSetting,
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

            //write the RedisData object to Cosmos DB using the IAsyncCollector
            await cosmosDBOut.AddAsync(redisData);
            logger.LogInformation($"Key: \"{newKey}\", Value: \"{redisData.value}\" added to Cosmos DB container: \"{Environment.GetEnvironmentVariable(containerSetting.Replace("%", ""))}\" at id: \"{redisData.id}\"");
        }

        /// <summary>
        /// Listens for messages sent on a Redis pub/sub channel and asynchronously writes the message and channel to Cosmos DB.
        /// </summary>
        /// <param name="pubSubMessage"> The message that was published to Redis.</param>
        /// <param name="cosmosDBOut"> An IAsyncCollector that is used to write the PubSubData to Cosmos DB.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        /// <returns></returns>
        [FunctionName(nameof(WriteBehindMessageAsync))]
        public static async Task WriteBehindMessageAsync(
            [RedisPubSubTrigger(redisConnectionSetting, pubSubChannelSetting)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: databaseSetting,
                containerName: pubSubContainerSetting,
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

            //write the PubSubData object to Cosmos DB using the IAsyncCollector
            await cosmosDBOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{Environment.GetEnvironmentVariable(pubSubContainerSetting.Replace("%", ""))}\" with id: \"{redisData.id}\"");
        }
    }
}
