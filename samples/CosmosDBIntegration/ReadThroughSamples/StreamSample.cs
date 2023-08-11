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
    internal class StreamSample
    {
        // Redis connection string and stream names stored in local.settings.json
        public const string RedisConnectionSetting = "RedisConnectionString";
        private static readonly Lazy<IDatabaseAsync> _redisDB = new Lazy<IDatabaseAsync>(() =>
           ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionSetting)).GetDatabase());
        public static string StreamNameSingleDocument = Environment.GetEnvironmentVariable("StreamTest");

        // CosmosDB connection string, database name and container name stored in local.settings.json
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSettingSingleDocument = "%StreamCosmosDbContainerIdSingleDocument%";

        /// <summary>
        /// Read Through: If the stream does not exist in Redis, refresh it with the values saved in CosmosDB 
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        /// <returns></returns>
        [FunctionName(nameof(ReadThroughForStreamSingleDocumentAsync))]
        public static async Task ReadThroughForStreamSingleDocumentAsync(
               [RedisPubSubTrigger(RedisConnectionSetting, "__keyevent@0__:keymiss")] string entry,
               [CosmosDB(
                    databaseName: DatabaseSetting,
                    containerName: ContainerSettingSingleDocument,
                    Connection = CosmosDbConnectionSetting)] CosmosClient cosmosDbClient,
               ILogger logger)
        {

            if (entry != StreamNameSingleDocument) return;
            
            // Connect CosmosDB container
            Container cosmosDbContainer = cosmosDbClient.GetContainer(Environment.GetEnvironmentVariable(DatabaseSetting.Replace("%", "")), Environment.GetEnvironmentVariable(ContainerSettingSingleDocument.Replace("%", "")));

            // Query Database by the stream name
            FeedIterator<StreamDataSingleDocument> query = cosmosDbContainer.GetItemLinqQueryable<StreamDataSingleDocument>(true)
                .Where(b => b.id == StreamNameSingleDocument)
                .ToFeedIterator();
            FeedResponse<StreamDataSingleDocument> response = await query.ReadNextAsync();
            StreamDataSingleDocument results = response.FirstOrDefault(defaultValue: null);

            // If stream not found
            if (results == null)
            {
                logger.LogWarning("{streamNameSingleDocument} was not found in the database, failed to read stream", StreamNameSingleDocument);
            }
            else
            {
                logger.LogInformation("{streamNameSingleDocument} was  found in the database", StreamNameSingleDocument);

                // Go through each message and format the key/value pairs
                foreach (var message in results.messages)
                {
                    var values = new NameValueEntry[message.Value.Count];
                    int i = 0;
                    foreach (var pair in message.Value)
                    {
                        values[i++] = new NameValueEntry(pair.Key, pair.Value);
                    }

                    // Upload value to Redis Stream
                    await _redisDB.Value.StreamAddAsync(StreamNameSingleDocument, values, messageId: message.Key, maxLength: results.maxlen);
                }
            }         
        }
    }
}
