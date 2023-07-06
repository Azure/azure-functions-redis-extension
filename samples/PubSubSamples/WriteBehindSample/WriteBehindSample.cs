using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class WriteBehindSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync("<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=").Result.GetDatabase();


        //write-behind caching: Write to Redis, then write to Cosmos asynchonously
        [FunctionName(nameof(WriteBehindAsync))]
        public static async Task WriteBehindAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<RedisData> cosmosOut,
            ILogger logger)
        {
            //load data from redis into a record
            RedisData redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: await s_redisDb.StringGetAsync(newKey),
                timestamp: DateTime.UtcNow
                );

            //write the record to cosmos
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"key: \"{newKey}\" value: \"{redisData.value}\" addedd to cosmosdb container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");
        }

    }
}
