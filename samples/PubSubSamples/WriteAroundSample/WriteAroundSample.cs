using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class WriteAroundSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable(localhostSetting)).Result.GetDatabase();


        //Write-Around caching: triggers when there is a direct write to Cosmos DB, then writes asynchronously to Redis
        [FunctionName(nameof(WriteAroundAsync))]
        public static async Task WriteAroundAsync([CosmosDBTrigger(
            databaseName: "DatabaseId",
            containerName: "ContainerId",
            Connection = cosmosDbConnectionSetting,
            LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<RedisData> input,
            ILogger log)
        {
            //if the list is empty, return
            if (input == null || input.Count <= 0) return;

            //for each item upladed to cosmos, write it to redis
            foreach (var document in input)
            {
                //if the key/value pair is already in Redis, throw an exception
                if (await s_redisDb.StringGetAsync(document.key) == document.value)
                {
                    throw new Exception($"The Key/Value pair is already in Azure Redis cache. ");
                }

                await s_redisDb.StringSetAsync(document.key, document.value);
                log.LogInformation($"Saved item with key: \"{document.key}\" and value: \"{document.value}\" in Azure Redis cache");
            }
        }
    }
}
