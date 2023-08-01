using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using StackExchange.Redis;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.VisualBasic;
using System.ComponentModel;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Functions.Worker.Extensions.Redis;
using Container = Microsoft.Azure.Cosmos.Container;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public record ListData
    (
        string id,
        List<string> value
    );
    public static class ListsSample
    {
        //Redis Cache primary connection string from local.settings.json
        public const string redisConnectionString = "redisConnectionString";

        //CosmosDB Endpoint from local.settings.json
        public const string CosmosDBConnectionString = "CosmosDBConnectionString";

        //CosmosDB database name and container name from local.settings.json
        public const string CosmosDbDatabaseId = "CosmosDbDatabaseId";
        public const string CosmosDbContainerId = "CosmosDbContainerId";

        //Uses the key of the user's choice and should be changed accordingly
        public const string key = "userListName";

        /// <summary>
        /// This function retrieves a specified item from a CosmosDB container and adds a new entry to it. The entry is retrieved from a Redis list trigger and added to the specified item's collection of values.
        /// </summary>
        /// <param name="listEntry">A string representing the value to be added to the CosmosDB item's collection.</param>
        /// <param name="client">>A Cosmos DB client object used to connect to the database.</param>
        /// <param name="logger">An ILogger object used for logging purposes.</param>
        /// <returns></returns>
        [FunctionName(nameof(ListTriggerWriteBehind))]
        public static async Task ListTriggerWriteBehind(
            [RedisListTrigger(redisConnectionString, key)] string listEntry, [CosmosDB(
            Connection = "CosmosDBConnectionString")]CosmosClient client,
            ILogger logger)
        {
            //Retrieve the database and container from the given client, which accesses the CosmosDB Endpoint
            Container db = client.GetDatabase(Environment.GetEnvironmentVariable(CosmosDbDatabaseId)).GetContainer(Environment.GetEnvironmentVariable(CosmosDbContainerId));

            //Creates query for item inthe container and
            //uses feed iterator to keep track of token when receiving results from query
            IOrderedQueryable<ListData> query = db.GetItemLinqQueryable<ListData>();
            using FeedIterator<ListData> results = query
                .Where(p => p.id == key)
                .ToFeedIterator();

            //Retrieve collection of items from results and then the first element of the sequence
            FeedResponse<ListData> response = await results.ReadNextAsync();
            ListData item = response.FirstOrDefault(defaultValue: null);

            //Optional logger to display what is being pushed to CosmosDB
            logger.LogInformation("The value added to " + key + " is " + listEntry + ". The value will be added to CosmosDB database: " + CosmosDbDatabaseId + " and container: " + CosmosDbContainerId + ".");

            //Create an entry if the key doesn't exist in CosmosDB or add to it if there is an existing entry
            List<string> resultsHolder = item?.value ?? new List<string>();

            resultsHolder.Add(listEntry);
            ListData newEntry = new ListData(id: key, value: resultsHolder);
            await db.UpsertItemAsync<ListData>(newEntry);
        }

    }
}