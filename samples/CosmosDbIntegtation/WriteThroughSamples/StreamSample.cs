﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class StreamSample
    {
        // Redis connection string and stream names stored in local.settings.json
        public const string redisConnectionSetting = "redisConnectionString";
        private const string streamName = "streamTest_WriteThrough";

        // CosmosDB connection string, client, database name and container name stored in local.settings.json
        public const string cosmosDbConnectionSetting = "cosmosDbConnectionString";
        private static CosmosClient cosmosDbClient = new(
            connectionString: Environment.GetEnvironmentVariable(cosmosDbConnectionSetting)!);
        public const string databaseSetting = "%cosmosDbDatabaseId%";
        public const string containerSetting = "%cosmosDbContainerId%";

        /// <summary>
        /// Write through: Write each message to it's own document in CosmosDB synchronously whenever a new value is added to the Redis Stream
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteThroughForStream))]
        public static void WriteThroughForStream(
                [RedisStreamTrigger(redisConnectionSetting, streamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: databaseSetting,
                    containerName: containerSetting,
                    Connection = cosmosDbConnectionSetting)] ICollector<StreamData> items,
                ILogger logger)
        {
            // Insert data into CosmosDB synchronously
            items.Add(StreamData.Format(entry, logger));
        }

        public const string containerSettingSingleDocument = "%cosmosDbContainerIdSingleDocument%";
        /// <summary>
        /// Write through (Single Document): Write messages to a single document in CosmosDB synchronously whenever a new value is added to the Redis Stream
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteThroughForStreamSingleDocument))]
        public static void WriteThroughForStreamSingleDocument(
                [RedisStreamTrigger(redisConnectionSetting, streamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: databaseSetting,
                    containerName: containerSettingSingleDocument,
                    Connection = cosmosDbConnectionSetting)] ICollector<StreamDataSingleDocument> items,
                 ILogger logger)
        {
            // Connect CosmosDB container
            Cosmos.Container cosmosDbContainer = cosmosDbClient.GetDatabase(Environment.GetEnvironmentVariable(databaseSetting.Substring(1, databaseSetting.Length -2)))
                .GetContainer(Environment.GetEnvironmentVariable(containerSettingSingleDocument.Substring(1, containerSettingSingleDocument.Length - 2)));

            // Query Database by the stream name
            StreamDataSingleDocument results = cosmosDbContainer.GetItemLinqQueryable<StreamDataSingleDocument>(true)
                                 .Where(b => b.id == streamName)
                                 .AsEnumerable()
                                 .FirstOrDefault();

            if (results == null)
            {
                // If the stream does not exist in CosmosDB, create a new entry
                StreamDataSingleDocument data = StreamDataSingleDocument.CreateNewEntry(entry, streamName, logger);

                // Insert data into CosmosDB synchronously
                items.Add(data);
            }
            else
            {
                // If the stream exists in CosmosDB, add to the existing entry
                StreamDataSingleDocument data = StreamDataSingleDocument.UpdateExistingEntry(results, entry, logger);

                // Insert data into CosmosDB synchronously
                items.Add(data);
            }
        }
    }
}
