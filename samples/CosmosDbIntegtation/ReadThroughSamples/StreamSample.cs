using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
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
        public const string redisConnectionSetting = "redisConnectionString";
        private static readonly Lazy<IDatabaseAsync> redisDB = new Lazy<IDatabaseAsync>(() =>
           ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionSetting)).GetDatabase());
        private const string streamNameSingleDocument = "streamTest_WriteBehind";

        // CosmosDB connection string, client, database name and container name stored in local.settings.json
        public const string cosmosDbConnectionSetting = "cosmosDbConnectionString";
        public const string databaseSetting = "%cosmosDbDatabaseId%";
        public const string containerSettingSingleDocument = "%cosmosDbContainerIdSingleDocument%";

        /// <summary>
        /// Read Through: If the stream does not exits, refresh it with the values saved in CosmosDB (only available if entries were written in a single document)
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        /// <returns></returns>
        [FunctionName(nameof(ReadThroughForStreamSingleDocumentAsync))]
        public static async Task ReadThroughForStreamSingleDocumentAsync(
               [RedisPubSubTrigger(redisConnectionSetting, "__keyevent@0__:keymiss")] string entry,
               [CosmosDB(
                    databaseName: databaseSetting,
                    containerName: containerSettingSingleDocument,
                    Connection = cosmosDbConnectionSetting)] CosmosClient cosmosDbClient,
               ILogger logger)
        {
            if (entry == streamNameSingleDocument)
            {
                // Connect CosmosDB container
                Cosmos.Container cosmosDbContainer = cosmosDbClient.GetDatabase(Environment.GetEnvironmentVariable(databaseSetting.Substring(1, databaseSetting.Length - 2)))
                    .GetContainer(Environment.GetEnvironmentVariable(containerSettingSingleDocument.Substring(1, containerSettingSingleDocument.Length - 2)));

                // Query Database by the stream name
                FeedIterator<StreamDataSingleDocument> query = cosmosDbContainer.GetItemLinqQueryable<StreamDataSingleDocument>(true)
                                     .Where(b => b.id == streamNameSingleDocument)
                                     .ToFeedIterator();

                FeedResponse<StreamDataSingleDocument> response = await query.ReadNextAsync();
                StreamDataSingleDocument results = response.FirstOrDefault(defaultValue: null);

                //If stream note found
                if (results == null)
                {
                    logger.LogInformation("stream not found");
                }
                else
                {
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
                        await redisDB.Value.StreamAddAsync(streamNameSingleDocument, values, messageId: message.Key, maxLength: results.maxlen);

                    }
                }
            }
        }
    }
}
