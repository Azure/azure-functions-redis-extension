using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    internal class PubSubCosmosIntegrationTestFunctions
    {
        //resolved from local.settings.json
        public const string localhostSetting = "RedisConnectionString";
        public const string cosmosDbConnectionSetting = "CosmosDBConnectionString";
        public const string cosmosDbDatabaseSetting = "CosmosDbDatabaseId";
        public const string cosmosDbContainerSetting = "CosmosDbContainerId";
        public const string pubSubContainerSetting = "PubSubContainerId";

        public const string pubsubChannel = "testChannel";
        public const string pubsubMultiple = "testChannel*";
        public const string keyspaceChannel = "__keyspace@0__:testKey";
        public const string keyspaceMultiple = "__keyspace@0__:testKey*";
        public const string keyeventChannelSet = "__keyevent@0__:set";
        public const string keyeventChannelAll = "__keyevent@0__:*";
        public const string keyspaceChannelAll = "__keyspace@0__:*";
        public const string allChannels = "*";


        private static readonly IDatabase s_redisDb =
            ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, localhostSetting)).GetDatabase();

        private static readonly Lazy<IDatabaseAsync> s_readRedisDb = new Lazy<IDatabaseAsync>(() =>
            ConnectionMultiplexer.ConnectAsync(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, localhostSetting)).Result.GetDatabase());

        // PubSubWrite -- Tests
        [FunctionName(nameof(SingleChannelWriteBehind))]
        public static async Task SingleChannelWriteBehind(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{redisData.id}\"");
        }

        
        [FunctionName(nameof(MultipleChannelWriteBehind))]
        public static async Task MultipleChannelWriteBehind(
            [RedisPubSubTrigger(localhostSetting, pubsubMultiple)] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{redisData.id}\"");
        }

        [FunctionName(nameof(AllChannelsWriteBehind))]
        public static async Task AllChannelsWriteBehind(
            [RedisPubSubTrigger(localhostSetting, allChannels)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{redisData.id}\"");
        }

        [FunctionName(nameof(SingleChannelWriteThrough))]
        public static void SingleChannelWriteThrough(
            [RedisPubSubTrigger(localhostSetting, pubsubChannel)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{cosmosDBOut.id}\"");
        }

        [FunctionName(nameof(MultipleChannelWriteThrough))]
        public static void MultipleChannelWriteThrough(
            [RedisPubSubTrigger(localhostSetting, pubsubMultiple)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{cosmosDBOut.id}\"");
        }

        [FunctionName(nameof(AllChannelsWriteThrough))]
        public static void AllChannelsWriteThrough(
            [RedisPubSubTrigger(localhostSetting, allChannels)] ChannelMessage pubSubMessage,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]out dynamic cosmosDBOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            cosmosDBOut = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            logger.LogInformation($"Message: \"{cosmosDBOut.message}\" from Channel: \"{cosmosDBOut.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{cosmosDBOut.id}\"");
        }


        //write-through -- Tests
        [FunctionName(nameof(WriteThrough))]
        public static void WriteThrough(
           [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:set")] string newKey,
           [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]out dynamic redisData,
           ILogger logger)
        {
            //assign the data from Redis to a dynamic object that will be written to Cosmos DB
            redisData = new RedisData(
                id: Guid.NewGuid().ToString(),
                key: newKey,
                value: s_redisDb.StringGet(newKey),
                timestamp: DateTime.UtcNow
            );

            logger.LogInformation($"Key: \"{newKey}\", Value: \"{redisData.value}\" addedd to Cosmos DB container: \"{"ContainerId"}\" at id: \"{redisData.id}\"");
        }

        //Write-Behind -- Tests
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

        //Write-Around -- Tests
        [FunctionName(nameof(WriteAroundAsync))]
        public static async Task WriteAroundAsync([CosmosDBTrigger(
            databaseName: "DatabaseId",
            containerName: "ContainerId",
            Connection = cosmosDbConnectionSetting,
            LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<RedisData> input,
            ILogger logger)
        {
            //if the list is empty, return
            if (input == null || input.Count <= 0) return;

            //for each item upladed to cosmos, write it to Redis
            foreach (var document in input)
            {
                //if the key/value pair is already in Redis, throw an exception
                if (await s_redisDb.StringGetAsync(document.key) == document.value)
                {
                    throw new Exception($"ERROR: Key: \"{document.key}\", Value: \"{document.value}\" pair is already in Azure Redis Cache.");
                }
                //Write the key/value pair to Redis
                await s_redisDb.StringSetAsync(document.key, document.value);
                logger.LogInformation($"Key: \"{document.key}\", Value: \"{document.value}\" added to Redis.");
            }
        }

        //Write-Around Message caching -- Tests
        [FunctionName(nameof(WriteAroundMessageAsync))]
        public static async Task WriteAroundMessageAsync(
            [CosmosDBTrigger(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<PubSubData> cosmosData,
            ILogger logger)
        {
            //if the list is empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            //for each new item upladed to cosmos, publish to Redis
            foreach (var document in cosmosData)
            {
                //publish the message to the correct Redis channel
                await s_redisDb.Multiplexer.GetSubscriber().PublishAsync(document.channel, document.message);
                logger.LogInformation($"Message: \"{document.message}\" has been published on channel: \"{document.channel}\".");
            }
        }

        //Read-Through -- Tests
        [FunctionName(nameof(ReadThroughAsync))]
        public static async Task ReadThroughAsync(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:keymiss")] string missedkey,
            [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
                Connection = cosmosDbConnectionSetting)]CosmosClient client,
            ILogger logger)
        {
            //get the Cosmos DB database and the container to read from
            Container cosmosContainer = client.GetContainer("DatabaseId", "ContainerId");
            var queryable = cosmosContainer.GetItemLinqQueryable<RedisData>();

            //get all entries in the container that contain the missed key
            using FeedIterator<RedisData> feed = queryable
                .Where(p => p.key == missedkey)
                .OrderByDescending(p => p.timestamp)
                .ToFeedIterator();
            var response = await feed.ReadNextAsync();

            //if the key is found in Cosmos DB, add  the most recently updated to Redis
            var item = response.FirstOrDefault(defaultValue: null);
            if (item != null)
            {
                await s_readRedisDb.Value.StringSetAsync(item.key, item.value);
                logger.LogInformation($"Key: \"{item.key}\", Value: \"{item.value}\" added to Redis.");
            }
            else
            {
                //if the key isnt found in Cosmos DB, throw an exception
                throw new Exception($"ERROR: Key: \"{missedkey}\" not found in Redis or Cosmos DB. Try adding the Key-Value pair to Redis or Cosmos DB.");
            }
        }
    }
}
