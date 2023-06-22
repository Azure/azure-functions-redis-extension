using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class PubSubSamples
    {
        public const string localhostSetting = "redisLocalhost";
        private static readonly CosmosClient _cosmosClient =
            new CosmosClient(connectionString: "AccountEndpoint=https://<cosmosdb-account>.documents.azure.com:443/;AccountKey=<access-key>;");
        private static readonly IDatabaseAsync _redisDb = 
            ConnectionMultiplexer.ConnectAsync("<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=").Result.GetDatabase();

        
        [FunctionName(nameof(WritePubSubMessageToCosmosAsync))]
        public static async Task WritePubSubMessageToCosmosAsync(
            [RedisPubSubTrigger(localhostSetting, "PubSubChannel")] string message,
            ILogger logger)
        {
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: "PubSubChannel",
                message: message,
                timestamp: DateTime.UtcNow
                );

            Container cosmosContainer = _cosmosClient.GetContainer("DatabaseId", "ContainerId");
            await cosmosContainer.UpsertItemAsync<PubSubData>(redisData);

            logger.LogInformation($"message: \"{message}\" from channel: \"{"PubSubChannel"}\" stored in cosmos container: \"{"ContainerId"}\" with id: \"{redisData.id}\"");
        }


        //keyspace notifications must be set to KEAm for this to trigger
        //read-through caching: Read from Redis, if not found, read from Cosmos, then write to Redis
        [FunctionName(nameof(ReadThroughAsync))]
        public static async Task ReadThroughAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:keymiss")] string missedkey,
            ILogger logger)
        {
            Container cosmosContainer = _cosmosClient.GetContainer("DatabaseId", "ContainerId");
            var queryable = cosmosContainer.GetItemLinqQueryable<RedisData>();

            //get the most recent entry in the database with the missed key
            using FeedIterator<RedisData> feed = queryable
                .Where(p => p.key == missedkey)
                .OrderByDescending(p => p.timestamp)
                .ToFeedIterator();
            var response = await feed.ReadNextAsync();

            //if the key is not found in cosmos, return
            var item = response.FirstOrDefault(defaultValue: null);
            if (item == null)
            {
                logger.LogInformation($"ERROR: key: \"{missedkey}\" not found in Redis or cosmosdb. Try adding the key-value pair to Redis or cosmos\n");
                return;
            }
            //if the key is found in cosmos, add it to Redis
            await _redisDb.StringSetAsync(item.key, item.value);
            
            logger.LogInformation($"key: \"{item.key}\" value: \"{item.value}\" addedd to Redis");

        }


        //write-through caching: Write to Redis, then write to Cosmos asynchonously
        [FunctionName(nameof(WriteThroughAsync))]
        public static async Task WriteThroughAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
            ILogger logger)
        {
            RedisData redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: await _redisDb.StringGetAsync(newKey),
                timestamp: DateTime.UtcNow
                );

            Container cosmosContainer = _cosmosClient.GetContainer("DatabaseId", "ContainerId");
            await cosmosContainer.UpsertItemAsync<RedisData>(redisData);

            logger.LogInformation($"key: \"{newKey}\" value: \"{redisData.value}\" addedd to cosmosdb container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");

        }

    }
}
