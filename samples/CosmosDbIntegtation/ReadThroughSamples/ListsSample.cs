﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class ListsSample
    {
        public record ListData
        (
            string id,
            List<string> value
        );

        //Redis Cache primary connection string from local.settings.json
        public const string redisConnectionString = "redisConnectionString";
        private static readonly IDatabase s_redisDb = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionString)).GetDatabase();

        //CosmosDB database name and container name from local.settings.json
        public const string CosmosDbDatabaseId = "CosmosDbDatabaseId";
        public const string CosmosDbContainerId = "CosmosDbContainerId";

        /// <summary>
        /// Adds a value to a Redis cache.
        /// </summary>
        /// <param name="response"> The response object returned by a Cosmos DB query. </param>
        /// <param name="item"> The item to be added to the Redis cache. </param>
        /// <param name="listEntry">The key for the Redis list to which the item will be added. </param>
        /// <returns> None </returns>
        public static async Task ToCacheAsync(FeedResponse<ListData> response, ListData item, string listEntry)
        {
            //Retrieve the values in cosmos associated with the list name, so you can access each item
            var fullEntry = response.Take(response.Count);

            if (fullEntry == null) return;

            //Accessing each value from the entry 
            foreach (ListData inputValues in fullEntry)
            {
                RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                await s_redisDb.ListRightPushAsync(listEntry, redisValues);

                //Optional foreach loop + console write line to confirm each value is sent to the cache
                foreach (RedisValue entryValue in redisValues)
                {
                    Console.WriteLine("Saved item " + entryValue + " in Azure Redis cache");

                }
            }
        }

        /// <summary>
        /// Function that retrieves a list from CosmosDB based on a Redis key miss event, and stores it in Redis cache using read-through caching. 
        /// The function takes a RedisPubSubTrigger attribute, which listens for key miss events on the Redis cache
        /// </summary>
        /// <param name="listEntry">The key for the value to be retrieved from Cosmos DB and added to the Redis cache.</param>
        /// <param name="client">A Cosmos DB client object used to connect to the database.</param>
        /// <param name="logger">An ILogger object used for logging purposes.</param>
        /// <returns></returns>
        [FunctionName(nameof(ListTriggerReadThroughFunc))]
        public static async Task ListTriggerReadThroughFunc(
            [RedisPubSubTrigger(redisConnectionString, "__keyevent@0__:keymiss")] string listEntry, [CosmosDB(
            Connection = "CosmosDBConnectionString" )]CosmosClient client,
            ILogger logger)
        {
            //Retrieve the database and container from the given client, which accesses the CosmosDB Endpoint
            Container db = client.GetDatabase(Environment.GetEnvironmentVariable(CosmosDbDatabaseId)).GetContainer(Environment.GetEnvironmentVariable(CosmosDbContainerId));

            //Creates query for item inthe container and
            //uses feed iterator to keep track of token when receiving results from query
            IOrderedQueryable<ListData> query = db.GetItemLinqQueryable<ListData>();
            using FeedIterator<ListData> results = query
                .Where(p => p.id == listEntry)
                .ToFeedIterator();

            //Retrieve collection of items from results and then the first element of the sequence
            FeedResponse<ListData> response = await results.ReadNextAsync();
            ListData item = response.FirstOrDefault(defaultValue: null);

            //If there doesnt exist an entry with this key in cosmos, no data will be retrieved
            if (item == null)
            {
                //Optional logger to display the name of the list trying to be retrieved is not in CosmosDB
                logger.LogInformation("This key does not exist in CosmosDB");
                return;
            }
            // If there exists an entry with this key in cosmos, 
            else
            {
                //Optional logger to display the name of the list trying to be retrieved
                logger.LogInformation("Found key: " + listEntry);

                await ToCacheAsync(response, item, listEntry);
            }
        }
    }
}