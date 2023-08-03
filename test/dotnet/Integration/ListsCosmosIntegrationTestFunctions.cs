using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Container = Microsoft.Azure.Cosmos.Container;
using System.Collections.Generic;



namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public static class ListsCosmosIntegrationTestFunctions
    {
        public const string redisConnectionString = "redisConnectionString";
        public const string CosmosDBConnectionString = "CosmosDBConnectionString";
        public const int pollingInterval = 100;
        private static readonly IDatabase cache = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionString)).GetDatabase();

        //CosmosDB database name and container name declared here
        public const string CosmosDbDatabaseId = "CosmosDbDatabaseId";
        public const string CosmosDbContainerId = "CosmosDbContainerId";

        //Uses the key of the user's choice and should be changed accordingly
        public const string key = "userListName";

        public static async Task ToCacheAsync(FeedResponse<CosmosDBListData> response, CosmosDBListData item, string listEntry)
        {
            var fullEntry = response.Take(response.Count);

            if (fullEntry == null) return;

            foreach (CosmosDBListData inputValues in fullEntry)
            {
                RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                await cache.ListRightPushAsync(listEntry, redisValues);
            }
        }

        [FunctionName("WriteAroundListTrigger")]
        public static void WriteAroundListTrigger([CosmosDBTrigger(
        databaseName: "%CosmosDbDatabaseId%",
        containerName: "%CosmosDbContainerId%",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases")]IReadOnlyList<CosmosDBListData> readOnlyList, ILogger log)
        {
            if (readOnlyList == null || readOnlyList.Count <= 0) return;

            foreach (CosmosDBListData inputValues in readOnlyList)
            {
                if (inputValues.id == key)
                {
                    RedisValue[] redisValues = Array.ConvertAll(inputValues.value.ToArray(), item => (RedisValue)item);
                    cache.ListRightPush(key, redisValues);
                }
            }
        }

        [FunctionName(nameof(ListTriggerWriteBehind))]
        public static async Task ListTriggerWriteBehind(
             [RedisListTrigger(redisConnectionString, key)] string listEntry, [CosmosDB(
            Connection = "CosmosDBConnectionString" )]CosmosClient client,
             ILogger logger)
        {
            Container db = client.GetDatabase(Environment.GetEnvironmentVariable(CosmosDbDatabaseId)).GetContainer(Environment.GetEnvironmentVariable(CosmosDbContainerId));

            IOrderedQueryable<CosmosDBListData> query = db.GetItemLinqQueryable<CosmosDBListData>();
            using FeedIterator<CosmosDBListData> results = query
                .Where(p => p.id == key)
                .ToFeedIterator();

            FeedResponse<CosmosDBListData> response = await results.ReadNextAsync();
            CosmosDBListData item = response.FirstOrDefault(defaultValue: null);

            List<string> resultsHolder = item?.value ?? new List<string>();

            resultsHolder.Add(listEntry);
            CosmosDBListData newEntry = new CosmosDBListData(id: key, value: resultsHolder);
            await db.UpsertItemAsync<CosmosDBListData>(newEntry);
        }

        [FunctionName(nameof(ListTriggerReadThroughFunc))]
        public static async Task ListTriggerReadThroughFunc(
            [RedisPubSubTrigger(redisConnectionString, "__keyevent@0__:keymiss")] string listEntry, [CosmosDB(
            Connection = "CosmosDBConnectionString" )]CosmosClient client,
            ILogger logger)
        {
            Container db = client.GetDatabase(Environment.GetEnvironmentVariable(CosmosDbDatabaseId)).GetContainer(Environment.GetEnvironmentVariable(CosmosDbContainerId));

            IOrderedQueryable<CosmosDBListData> query = db.GetItemLinqQueryable<CosmosDBListData>();
            using FeedIterator<CosmosDBListData> results = query
                .Where(p => p.id == listEntry)
                .ToFeedIterator();

            FeedResponse<CosmosDBListData> response = await results.ReadNextAsync();
            CosmosDBListData item = response.FirstOrDefault(defaultValue: null);

            if (item == null) return;
            else
            {
                await ToCacheAsync(response, item, listEntry);
            }
        }
    }
}