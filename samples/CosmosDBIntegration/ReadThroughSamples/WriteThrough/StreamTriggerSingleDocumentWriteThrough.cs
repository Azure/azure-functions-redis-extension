using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteThrough
{
    internal class StreamTriggerSingleDocumentWriteThrough
    {
        // Redis connection string and stream names stored in local.settings.json
        public const string RedisConnectionSetting = "RedisConnectionString";
        public const string StreamName = "%StreamTest%";

        // CosmosDB connection string, client, database name and container name stored in local.settings.json
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";
        private static CosmosClient _cosmosDbClient = new(
            connectionString: Environment.GetEnvironmentVariable(CosmosDbConnectionSetting)!);
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%StreamCosmosDbContainerId%";
        public const string ContainerSettingSingleDocument = "%StreamCosmosDbContainerIdSingleDocument%";

        /// <summary>
        /// Write through (Single Document): Write messages to a single document in CosmosDB synchronously whenever a new value is added to the Redis Stream.
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(StreamTriggerSingleDocumentWriteThrough))]
        public static void Run(
                [RedisStreamTrigger(RedisConnectionSetting, StreamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: DatabaseSetting,
                    containerName: ContainerSettingSingleDocument,
                    Connection = CosmosDbConnectionSetting)] ICollector<StreamDataSingleDocument> items,
                 ILogger logger)
        {
            string stream = Environment.GetEnvironmentVariable(StreamName.Replace("%", ""));

            // Connect CosmosDB container
            Container cosmosDbContainer = _cosmosDbClient.GetContainer(Environment.GetEnvironmentVariable(DatabaseSetting.Replace("%", "")), Environment.GetEnvironmentVariable(ContainerSettingSingleDocument.Replace("%", "")));

            // Query CosmosDB database by the stream name
            StreamDataSingleDocument results = cosmosDbContainer.GetItemLinqQueryable<StreamDataSingleDocument>(true)
                    .Where(b => b.id == stream)
                    .AsEnumerable()
                    .FirstOrDefault();

            if (results == null)
            {
                // If the stream does not exist in CosmosDB, create a new entry and insert into CosmosDB synchronously
                StreamDataSingleDocument data = StreamDataSingleDocument.CreateNewEntry(entry, stream, logger);
                items.Add(data);
            }
            else
            {
                // If the stream exists in CosmosDB, add to the existing entry and insert into CosmosDB synchronously
                StreamDataSingleDocument data = StreamDataSingleDocument.UpdateExistingEntry(results, entry, logger);
                items.Add(data);
            }
        }
    }
}
