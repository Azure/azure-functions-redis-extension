using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

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

        private static readonly IConnectionMultiplexer s_redisConnection =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionSetting));


        /// <summary>
        /// Triggers when key/value pairs are written directly to Cosmos DB then asynchronously writes the associated key/value pair to Redis.
        /// </summary>
        /// <param name="cosmosData"> An IReadonlyList containing all the new/updated documents in the specified Cosmos DB container.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        /// <returns></returns>
        /// <exception cref="Exception"> Thrown when they key/value pair is already stored in the Redis cache.</exception>
        [FunctionName(nameof(WriteAroundAsync))]
        public static async Task WriteAroundAsync(
            [CosmosDBTrigger(
                databaseName: databaseSetting,
                containerName: containerSetting,
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<RedisData> cosmosData, 
            ILogger logger)
        {
            //if the list is null or empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            IDatabaseAsync s_redisDb = s_redisConnection.GetDatabase();
            //for each item upladed to cosmos, write it to Redis
            foreach (var document in cosmosData)
            {
                //if the key/value pair is already in Redis, throw an exception
                if (await s_redisDb.StringGetAsync(document.key) == document.value)
                {
                    throw new Exception($"ERROR: Key: \"{document.key}\", Value: \"{document.value}\" pair is already in Azure Redis Cache.");
                }
                //Write the key/value pair to Redis
                await s_redisDb.StringSetAsync(document.key, document.value);
                logger.LogInformation($"Key: \"{document.key}\", Value: \"{document.value}\" added to Redis.");
            }
        }

        /// <summary>
        /// Triggers when pubsub messages are written directly to Cosmos DB, then asynchronously publishes the associated message to the Redis channel that was specified.
        /// </summary>
        /// <param name="cosmosData"> A readonly list containing all the new/updated documents in the specified Cosmos DB container.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        /// <returns></returns>
        [FunctionName(nameof(WriteAroundMessageAsync))]
        public static async Task WriteAroundMessageAsync(
            [CosmosDBTrigger(
                databaseName: databaseSetting,
                containerName: pubSubContainerSetting,
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<PubSubData> cosmosData,
            ILogger logger)
        {
            //if the list is null or empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            ISubscriber s_redisPublisher = s_redisConnection.GetSubscriber();
            //for each new item upladed to cosmos, publish to Redis
            foreach (var document in cosmosData)
            {
                //publish the message to the correct Redis channel
                await s_redisPublisher.PublishAsync(document.channel, document.message);
                logger.LogInformation($"Message: \"{document.message}\" has been published on channel: \"{document.channel}\".");
            }
        }
    }
}
