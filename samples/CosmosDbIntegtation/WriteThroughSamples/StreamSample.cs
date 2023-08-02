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
        public const string redisLocalHost = "redisConnectionString";
        public const string streamName = "streamTest";

        // CosmosDB connection string, database name and container name stored in local.settings.json
        public const string cosmosConnectionString = "cosmosConnectionString";
        public const string cosmosDatabase = "%database-id%";
        public const string cosmosContainer = "%container-id%";

        /// <summary>
        /// Write through: Write to CosmosDB synchronously whenever a new value is added to the Redis Stream
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteThroughForStream))]
        public static void WriteThroughForStream(
                [RedisStreamTrigger(redisLocalHost, streamName)] StreamEntry entry,
                 [CosmosDB(
                databaseName: "database-id",
                containerName: "container-id",
                Connection = "cosmosConnectionString")]
                ICollector<CosmosDBData> items,
                ILogger logger)
        {
            // Insert data into CosmosDB synchronously
            items.Add(CosmosDBData.Format(entry, logger));
        }
    }
}
