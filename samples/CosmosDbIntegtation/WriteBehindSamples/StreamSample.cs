using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteBehindSamples
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
        /// Write behind: Write to CosmosDB asynchronously whenever a new value is added to the Redis Stream
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteBehindAsync))]
        public static async Task WriteBehindAsync(
                [RedisStreamTrigger(redisLocalHost, streamName)] StreamEntry entry,
                [CosmosDB(
                databaseName: cosmosDatabase,
                containerName: cosmosContainer,
                Connection = cosmosConnectionString)]
                IAsyncCollector<Data> items,
                ILogger logger)
        {
            // Insert data into CosmosDB asynchronously
            await items.AddAsync(FormatData(entry, logger));
        }

        // Helper method to format stream message
        private static Data FormatData(StreamEntry entry, ILogger logger)
        {
            logger.LogInformation("ID: {val}", entry.Id.ToString());

            // Map each key value pair
            Dictionary<string, string> dict = RedisUtilities.StreamEntryToDictionary(entry);

            Data sampleItem = new Data { id = entry.Id, values = dict };
            return sampleItem;
        }
    }
}

public class Data
{
    public string id { get; set; }
    public Dictionary<string, string> values { get; set; }
}
