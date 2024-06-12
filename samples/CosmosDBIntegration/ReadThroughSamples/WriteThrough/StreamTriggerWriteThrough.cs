using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteThrough
{
    internal class StreamTriggerWriteThrough
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

        /// <summary>
        /// Write through: Write messages to CosmosDB synchronously whenever a new value is added to the Redis Stream. Each message will get it's own document.
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(StreamTriggerWriteThrough))]
        public static void Run(
                [RedisStreamTrigger(RedisConnectionSetting, StreamName)] StreamEntry entry,
                [CosmosDB(
                    databaseName: DatabaseSetting,
                    containerName: ContainerSetting,
                    Connection = CosmosDbConnectionSetting)] ICollector<StreamData> items,
                ILogger logger)
        {
            // Insert data into CosmosDB synchronously
            items.Add(StreamData.Format(entry, logger));
        }
    }
}
