using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteAround
{
    internal class StreamSingleDocumentWriteAround
    {
        // Redis database and stream stored in local.settings.json
        public const string RedisConnectionSetting = "RedisConnectionString";
        private static readonly Lazy<IDatabaseAsync> _redisDB = new Lazy<IDatabaseAsync>(() =>
           ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionSetting)).GetDatabase());
        public static string StreamName = Environment.GetEnvironmentVariable("StreamTest");

        // CosmosDB connection string, database name and container name stored in local.settings.json
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%StreamCosmosDbContainerId%";
        public const string ContainerSettingSingleDocument = "%StreamCosmosDbContainerIdSingleDocument%";
        public static string StreamNameSingleDocument = Environment.GetEnvironmentVariable("StreamTestSingleDocument");

        /// <summary>
        /// Write Around (Single Document): Write from Cosmos DB to Redis whenever a change occurs in one of the CosmosDB documents
        /// </summary>
        /// <param name="input"> List of changed documents in CosmosDB </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(StreamSingleDocumentWriteAround))]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: DatabaseSetting,
                containerName: ContainerSettingSingleDocument,
                Connection = CosmosDbConnectionSetting,
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)]IReadOnlyList<StreamDataSingleDocument> input, ILogger logger)
        {
            if (input == null) return;

            // Iterate through each changed document
            foreach (var document in input)
            {
                logger.LogInformation("{stream1} changed and sent to {stream2} ", document.id, StreamNameSingleDocument);

                // Go through each message and format the key/value pairs
                foreach (var message in document.messages)
                {
                    var values = new NameValueEntry[message.Value.Count];
                    int i = 0;
                    foreach (var entry in message.Value)
                    {
                        values[i++] = new NameValueEntry(entry.Key, entry.Value);
                    }

                    // Upload value to Redis Stream
                    await _redisDB.Value.StreamAddAsync(StreamNameSingleDocument, values, messageId: message.Key, maxLength: document.maxlen);
                }
            }
        }
    }
}
