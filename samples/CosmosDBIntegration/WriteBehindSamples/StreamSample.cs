using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
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
