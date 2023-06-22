using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSamples
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync("<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=").Result.GetDatabase();


        [FunctionName(nameof(WritePubSubMessageToCosmosAsync))]
        public static async Task WritePubSubMessageToCosmosAsync(
            [RedisPubSubTrigger(localhostSetting, "PubSubChannel")] string message,
             [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosOut,
            ILogger logger)
        {
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: "PubSubChannel",
                message: message,
                timestamp: DateTime.UtcNow
                );
            
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"message: \"{message}\" from channel: \"{"PubSubChannel"}\" stored in cosmos container: \"{"ContainerId"}\" with id: \"{redisData.id}\"");
        }

        //keyspace notifications must be set to KEAm for this to trigger
        //read-through caching: Read from Redis, if not found, read from Cosmos, then write to Redis
        [FunctionName(nameof(ReadThroughAsync))]
        public static async Task ReadThroughAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:keymiss")] string missedkey,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]CosmosClient client,
            ILogger logger)
        {
            Container cosmosContainer = client.GetContainer("DatabaseId", "ContainerId");
            var queryable = cosmosContainer.GetItemLinqQueryable<RedisData>();

            //get all entries in the database that contain the missed key
            using FeedIterator<RedisData> feed = queryable
                .Where(p => p.key == missedkey)
                .OrderByDescending(p => p.timestamp)
                .ToFeedIterator();
            var response = await feed.ReadNextAsync();

            //if the key is found in cosmos, add  the most recently updated to Redis
            var item = response.FirstOrDefault(defaultValue: null);
            if (item != null)
            {
                await s_redisDb.StringSetAsync(item.key, item.value);
                logger.LogInformation($"key: \"{item.key}\" value: \"{item.value}\" addedd to Redis");
            }
            else
            {
                //if the key isnt found in cosmos, throw an exception
                throw new Exception($"ERROR: key: \"{missedkey}\" not found in Redis or cosmosdb. Try adding the key-value pair to Redis or cosmos\n");
            } 
        }

        //write-through caching: Write to Redis, then write to Cosmos asynchonously
        [FunctionName(nameof(WriteThroughAsync))]
        public static async Task WriteThroughAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey, 
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<RedisData> cosmosOut,
            ILogger logger)
        {

            RedisData redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: await s_redisDb.StringGetAsync(newKey),
                timestamp: DateTime.UtcNow
                );
            
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"key: \"{newKey}\" value: \"{redisData.value}\" addedd to cosmosdb container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");
        }
    }
}