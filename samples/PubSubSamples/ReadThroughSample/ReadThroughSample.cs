using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class ReadThroughSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync("<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=").Result.GetDatabase();


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
            //get the cosmos database and container to read from
            Container cosmosContainer = client.GetContainer("DatabaseId", "ContainerId");
            var queryable = cosmosContainer.GetItemLinqQueryable<RedisData>();

            //get all entries in the container that contain the missed key
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
                throw new Exception($"ERROR: key: \"{missedkey}\" not found in Redis or cosmosdb. Try adding the key-value pair to Redis or Cosmos");
            }
        }
    }
}
