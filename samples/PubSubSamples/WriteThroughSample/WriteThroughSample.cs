using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class WriteThroughSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync("<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=").Result.GetDatabase();


        //write-through caching: Write to Redis then synchronously write to CosmosDB
        [FunctionName(nameof(WriteThrough))]
        public static void WriteThrough(
           [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
           [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]out dynamic redisData,
           ILogger logger)
        {
          //get the redisDB synchronously
            IDatabase redisDb = s_redisDb.Multiplexer.GetDatabase();

            //assign the data from redis to a dyncmic object that will be written to cosmos
            redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: redisDb.StringGet(newKey),
                timestamp: DateTime.UtcNow
            );

            logger.LogInformation($"key: \"{newKey}\" value: \"{redisData.value}\" addedd to cosmosdb container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");
        }
    }
}
