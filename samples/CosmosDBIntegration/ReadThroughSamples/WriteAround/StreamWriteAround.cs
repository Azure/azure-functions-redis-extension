using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteAround
{
    internal class StreamWriteAround
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

        /// <summary>
        /// Write Around: Write from Cosmos DB to Redis whenever a change occurs in one of the CosmosDB documents
        /// </summary>
        /// <param name="input"> List of changed documents in CosmosDB </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(StreamWriteAround))]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: DatabaseSetting,
                containerName: ContainerSetting,
                Connection = CosmosDbConnectionSetting,
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)]IReadOnlyList<StreamData> input, ILogger logger)
        {
            if (input == null) return;

            // Iterate through each changed document
            foreach (var document in input)
            {
                logger.LogInformation("{messageID} changed and sent to {stream} ", document.id, StreamName);

                var values = new NameValueEntry[document.values.Count];
                int i = 0;

                // Format the key/value pairs
                foreach (KeyValuePair<string, string> entry in document.values)
                {
                    values[i++] = new NameValueEntry(entry.Key, entry.Value);
                }

                // Upload value to Redis Stream
                await _redisDB.Value.StreamAddAsync(StreamName, values);
            }
        }
    }
}
