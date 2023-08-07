using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class ListSample
    {
        //Redis Cache primary connection string from local.settings.json
        public const string redisConnectionString = "redisConnectionString";
        private static readonly IDatabase s_redisDb = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionString)).GetDatabase();

        //CosmosDB Endpoint from local.settings.json
        public const string CosmosDBConnectionString = "CosmosDBConnectionString";

        //Uses the key of the user's choice and should be changed accordingly
        public const string key = "userListName";

        /// <summary>
        /// This function is triggered by changes to a specified CosmosDB container. It retrieves a list of items that have been modified or added 
        /// to the container and adds them to a Redis cache. The function converts each item's collection of values into an array and pushes the array to the Redis cache.
        /// </summary>
        /// <param name="readOnlyList">An IReadOnlyList of ListData objects representing the items that have been modified or added to the CosmosDB container.</param>
        /// <param name="log">An ILogger object used for logging purposes.</param>
        [FunctionName("WriteAroundListTrigger")]
        public static void WriteAroundListTrigger([CosmosDBTrigger(
        databaseName: "%CosmosDbDatabaseId%",
        containerName: "%CosmosDbContainerId%",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases")]IReadOnlyList<CosmosDBListData> readOnlyList, ILogger log)
        {
            if (readOnlyList == null || readOnlyList.Count <= 0) return;

            //Accessing each entry from readOnlyList
            foreach (CosmosDBListData inputValues in readOnlyList)
            {
                if (inputValues.id == key)
                {
                    //Converting one entry into an array format
                    RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                    s_redisDb.ListRightPush(key, redisValues);

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