using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";
        public const string cosmosDBDatabaseName = "DatabaseId";
        public const string cosmosDBContainerName = "ContainerId";
        public const string cosmosDBPubSubContainerName = "PSContainerId";

        private static readonly IConnectionMultiplexer s_redisConnection =
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(localhostSetting));

        private static readonly IDatabaseAsync s_redisDb = s_redisConnection.GetDatabase();
        private static readonly ISubscriber s_redisPublisher = s_redisConnection.GetSubscriber();


        //Write-Around caching: triggers when there is a direct write to Cosmos DB, then writes asynchronously to Redis
        [FunctionName(nameof(WriteAroundAsync))]
        public static async Task WriteAroundAsync(
            [CosmosDBTrigger(
                databaseName: cosmosDBDatabaseName,
                containerName: cosmosDBContainerName,
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<RedisData> cosmosData, 
            ILogger logger)
        {
            //if the list is empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            //for each item upladed to cosmos, write it to Redis
            foreach (var document in cosmosData)
            {
                //if the key/value pair is already in Redis, throw an exception
                if (await s_redisConnection.GetDatabase().StringGetAsync(document.key) == document.value)
                {
                    throw new Exception($"ERROR: Key: \"{document.key}\", Value: \"{document.value}\" pair is already in Azure Redis Cache.");
                }
                //Write the key/value pair to Redis
                await s_redisDb.StringSetAsync(document.key, document.value);
                logger.LogInformation($"Key: \"{document.key}\", Value: \"{document.value}\" added to Redis.");
            }
        }
        //Write-Around caching: triggers when there is a direct write to Cosmos DB's pubsub container, then writes the message to the Redis channel that was specified
        [FunctionName(nameof(WriteAroundMessageAsync))]
        public static async Task WriteAroundMessageAsync(
            [CosmosDBTrigger(
                databaseName: cosmosDBDatabaseName,
                containerName: cosmosDBPubSubContainerName,
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<PubSubData> cosmosData,
            ILogger logger)
        {
            //if the list is empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

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
