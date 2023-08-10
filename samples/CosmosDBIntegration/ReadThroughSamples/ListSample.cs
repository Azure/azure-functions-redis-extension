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
    public static class ListSample
    {
        //Redis Cache primary connection string from local.settings.json
        public const string RedisConnectionString = "RedisConnectionString";
        private static readonly Lazy<IDatabaseAsync> s_redisDb = new Lazy<IDatabaseAsync>(() => ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionString)).GetDatabase());

        //CosmosDB database name and container name from local.settings.json
        public const string CosmosDbDatabaseId = "CosmosDbDatabaseId";
        public const string CosmosDbContainerId = "ListCosmosDbContainerId";

        //Uses the key of the user's choice and should be changed accordingly
        public const string ListKey = "userListName";

        /// <summary>
        /// Adds a CosmosDBListData item to a Redis list with a specific key.
        /// </summary>
        /// <param name="response"> The response object returned by a Cosmos DB query. </param>
        /// <param name="item"> The item to be added to the Redis cache. </param>
        /// <param name="listEntry">The key for the Redis list to which the item will be added. </param>
        /// <returns> None </returns>
        public static async Task ToCacheAsync(FeedResponse<CosmosDBListData> response, CosmosDBListData item, string listEntry)
        {
            //Retrieve the values in cosmos associated with the list name, so you can access each item
            var fullEntry = response.Take(response.Count);

            if (fullEntry == null) return;

            //Accessing each value from the entry 
            foreach (CosmosDBListData inputValues in fullEntry)
            {
                RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                await s_redisDb.Value.ListRightPushAsync(listEntry, redisValues);
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
            [RedisPubSubTrigger(RedisConnectionString, "__keyevent@0__:keymiss")] string listEntry, [CosmosDB(
            Connection = "CosmosDBConnectionString" )]CosmosClient client,
            ILogger logger)
        {
            if (listEntry != ListKey) return;
            //Retrieve the database and container from the given client, which accesses the CosmosDB Endpoint
            Container cosmosDbContainer = client.GetDatabase(Environment.GetEnvironmentVariable(CosmosDbDatabaseId)).GetContainer(Environment.GetEnvironmentVariable(CosmosDbContainerId));

            //Creates query for item inthe container and
            //uses feed iterator to keep track of token when receiving results from query
            IOrderedQueryable<CosmosDBListData> query = cosmosDbContainer.GetItemLinqQueryable<CosmosDBListData>();
            using FeedIterator<CosmosDBListData> results = query
                .Where(p => p.id == listEntry)
                .ToFeedIterator();

            //Retrieve collection of items from results and then the first element of the sequence
            FeedResponse<CosmosDBListData> response = await results.ReadNextAsync();
            CosmosDBListData item = response.FirstOrDefault(defaultValue: null);

            //If there doesnt exist an entry with this key in cosmos, no data will be retrieved
            if (item == null)
            {
                //Optional logger to display the name of the list trying to be retrieved is not in CosmosDB
                logger.LogInformation("This key does not exist in CosmosDB");
                return;
            }
            //If there exists an entry with this key in cosmos, 
            else
            {
                //Optional logger to display the name of the list trying to be retrieved
                logger.LogInformation("Found key: " + listEntry);

                await ToCacheAsync(response, item, listEntry);
            }
        }
    }
}