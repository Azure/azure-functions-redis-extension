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
            ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable(localhostSetting)).Result.GetDatabase();


        //write-behind caching: Write to Redis, then write to Cosmos DB asynchronously
        [FunctionName(nameof(WriteBehindAsync))]
        public static async Task WriteBehindAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<RedisData> cosmosOut,
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
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"Key: \"{newKey}\", Value: \"{redisData.value}\" added to Cosmos DB container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");
        }

    }
}
