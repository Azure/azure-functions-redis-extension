<<<<<<< HEAD
﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class StreamSample
    {
        // Redis connection string and stream names stored in local.settings.json
        public const string RedisConnectionSetting = "RedisConnectionString";
        public const string StreamName = "%StreamTest%";

        // CosmosDB connection string, client, database name and container name stored in local.settings.json
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";
        private static CosmosClient _cosmosDbClient = new(
            connectionString: Environment.GetEnvironmentVariable(CosmosDbConnectionSetting)!);
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%CosmosDbContainerId%";

        /// <summary>
        /// Write behind: Write messages to CosmosDB asynchronously whenever a new value is added to the Redis Stream. Each message will get it's own document.
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteBehindForStream))]
        public static async Task WriteBehindForStream(
                [RedisStreamTrigger(RedisConnectionSetting, StreamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: DatabaseSetting,
                    containerName: ContainerSetting,
                    Connection = CosmosDbConnectionSetting)] IAsyncCollector<StreamData> items,
                ILogger logger)
        {
            // Insert data into CosmosDB asynchronously
            await items.AddAsync(StreamData.Format(entry, logger));
        }

        public const string ContainerSettingSingleDocument = "%CosmosDbContainerIdSingleDocument%";

        /// <summary>
        /// Write behind (Single Document): Write messages to a single document in CosmosDB asynchronously whenever a new value is added to the Redis Stream.
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteBehindForStreamSingleDocumentAsync))]
        public static async Task WriteBehindForStreamSingleDocumentAsync(
                [RedisStreamTrigger(RedisConnectionSetting, StreamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: DatabaseSetting,
                    containerName: ContainerSettingSingleDocument,
                    Connection = CosmosDbConnectionSetting)] IAsyncCollector<StreamDataSingleDocument> items,
                 ILogger logger)
        {
            string stream = Environment.GetEnvironmentVariable(StreamName.Replace("%", ""));

            // Connect CosmosDB container
            Container cosmosDbContainer = _cosmosDbClient.GetContainer(Environment.GetEnvironmentVariable(DatabaseSetting.Replace("%", "")), Environment.GetEnvironmentVariable(ContainerSettingSingleDocument.Replace("%", "")));

            // Query CosmosDB database by the stream name
            FeedIterator<StreamDataSingleDocument> query = cosmosDbContainer.GetItemLinqQueryable<StreamDataSingleDocument>(true)
                    .Where(b => b.id == stream)
                    .ToFeedIterator();

            FeedResponse<StreamDataSingleDocument> response = await query.ReadNextAsync();
            StreamDataSingleDocument results = response.FirstOrDefault(defaultValue: null);       

            if (results == null)
            {
                // If the stream does not exist in CosmosDB, create a new entry and insert into CosmosDB asynchronously
                StreamDataSingleDocument data = StreamDataSingleDocument.CreateNewEntry(entry, stream, logger);
                await items.AddAsync(data);
            }
            else
            {
                // If the stream exists in CosmosDB, add to the existing entry and insert into CosmosDB asynchronously
                StreamDataSingleDocument data = StreamDataSingleDocument.UpdateExistingEntry(results, entry, logger);
                await items.AddAsync(data);
            }
        }
    }
}
=======
﻿using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class StreamSample
    {
        // Redis connection string and stream names stored in local.settings.json
        public const string redisConnectionSetting = "redisConnectionString";
        public const string streamName = "streamTest";

        // CosmosDB connection string, client, database name and container name stored in local.settings.json
        public const string cosmosDbConnectionSetting = "cosmosDbConnectionString";
        public const string databaseSetting = "%cosmosDbDatabaseId%";
        public const string containerSetting = "%cosmosDbContainerId%";

        /// <summary>
        /// Write behind: Write to CosmosDB asynchronously whenever a new value is added to the Redis Stream
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteBehindForStream))]
        public static async Task WriteBehindForStream(
                [RedisStreamTrigger(redisConnectionSetting, streamName)] StreamEntry entry,
                [CosmosDB(
                databaseName: databaseSetting,
                containerName: containerSetting,
                Connection = cosmosDbConnectionSetting)]
                IAsyncCollector<CosmosDBData> items,
                ILogger logger)
        {
            // Insert data into CosmosDB asynchronously
            await items.AddAsync(CosmosDBData.Format(entry, logger));
        }
    }
}
<<<<<<< HEAD
>>>>>>> bad07ae (Fixed spelling error for naming of parent directory and reduced duplicate files for custom data (#101))
=======
>>>>>>> 58a8a06 (continued rebase, added readme.md and tutorial.md)
>>>>>>> 61a4462 (continued rebase)
