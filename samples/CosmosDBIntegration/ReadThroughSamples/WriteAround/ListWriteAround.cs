using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteAround
{
    public static class ListWriteAround
    {
        //Redis Cache primary connection string from local.settings.json
        public const string RedisConnectionString = "RedisConnectionString";
        private static readonly IDatabase s_redisDb = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionString)).GetDatabase();

        //CosmosDB Endpoint from local.settings.json
        public const string CosmosDBConnectionString = "CosmosDBConnectionString";

        //Uses the key of the user's choice and should be changed accordingly
        public const string ListKey = "userListName";

        /// <summary>
        /// This function is triggered by changes to a specified CosmosDB container. It retrieves a list of items that have been modified or added
        /// to the container and adds them to a Redis cache. The function converts each item's collection of values into an array and pushes the array to the Redis cache.
        /// </summary>
        /// <param name="readOnlyList">An IReadOnlyList of ListData objects representing the items that have been modified or added to the CosmosDB container.</param>
        /// <param name="log">An ILogger object used for logging purposes.</param>
        [FunctionName(nameof(ListWriteAround))]
        public static void Run([CosmosDBTrigger(
            databaseName: "%CosmosDbDatabaseId%",
            containerName: "%ListCosmosDbContainerId%",
            Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases")]IReadOnlyList<CosmosDBListData> readOnlyList, ILogger log)
        {
            if (readOnlyList == null || readOnlyList.Count <= 0) return;

            //Accessing each entry from readOnlyList
            foreach (CosmosDBListData inputValues in readOnlyList)
            {
                if (inputValues.id == ListKey)
                {
                    //Converting one entry into an array format
                    RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                    s_redisDb.ListRightPush(ListKey, redisValues);

                    //Optional foreach loop + log to confirm each value is sent to the cache
                    foreach (RedisValue entryValue in redisValues)
                    {
                        log.LogInformation("Saved item " + entryValue + " in Azure Redis cache");

                    }
                }
            }
        }
    }
}