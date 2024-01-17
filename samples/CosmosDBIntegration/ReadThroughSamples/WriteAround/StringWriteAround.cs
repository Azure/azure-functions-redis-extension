using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteAround
{
    public static class StringWriteAround
    {
        //Connection string settings that will be resolved from local.settings.json file
        public const string RedisConnectionSetting = "RedisConnectionString";
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%PubSubCosmosDbContainerId%";
        public const string PubSubContainerSetting = "%MessagesCosmosDbContainerId%";

        private static readonly Lazy<IConnectionMultiplexer> s_redisConnection = new Lazy<IConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionSetting)));


        /// <summary>
        /// Triggers when key/value pairs are written directly to Cosmos DB then asynchronously writes the associated key/value pair to Redis.
        /// </summary>
        /// <param name="cosmosData"> An IReadonlyList containing all the new/updated documents in the specified Cosmos DB container.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        /// <returns></returns>
        /// <exception cref="Exception"> Thrown when they key/value pair is already stored in the Redis cache.</exception>
        [FunctionName(nameof(StringWriteAround))]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: DatabaseSetting,
                containerName: ContainerSetting,
                Connection = CosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<RedisData> cosmosData,
            ILogger logger)
        {
            //if the list is null or empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            IDatabaseAsync redisDb = s_redisConnection.Value.GetDatabase();
            //for each item upladed to cosmos, write it to Redis
            foreach (var document in cosmosData)
            {
                //if the key/value pair is already in Redis, throw an exception
                if (await redisDb.StringGetAsync(document.key) == document.value)
                {
                    throw new Exception($"ERROR: Key: \"{document.key}\", Value: \"{document.value}\" pair is already in Azure Redis Cache.");
                }
                //Write the key/value pair to Redis
                await redisDb.StringSetAsync(document.key, document.value);
                logger.LogInformation($"Key: \"{document.key}\", Value: \"{document.value}\" added to Redis.");
            }
        }
    }
}
