using Microsoft.Azure.WebJobs;
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
        // Redis database and stream stored in local.settings.json
        public const string redisConnectionSetting = "redisConnectionString";
        private static readonly Lazy<IDatabaseAsync> redisDB = new Lazy<IDatabaseAsync>(() =>
           ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(redisConnectionSetting)).GetDatabase());
        public const string stream = "streamTest";

        // CosmosDB connection string, database name and container name stored in local.settings.json
        public const string cosmosDbConnectionSetting = "cosmosDbConnectionString";
        public const string databaseSetting = "%cosmosDbDatabaseId%";
        public const string containerSetting = "%cosmosDbContainerId%";

        /// <summary>
        /// Write Around: Write from Cosmos DB to Redis whenever a change occurs in one of the CosmosDB documents
        /// </summary>
        /// <param name="input"> List of changed documents in CosmosDB </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteAroundForStreamAsync))]
        public static async Task WriteAroundForStreamAsync(
            [CosmosDBTrigger(
                databaseName: databaseSetting,
                containerName: containerSetting,
                Connection = cosmosDbConnectionSetting,
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)]IReadOnlyList<CosmosDBData> input, ILogger logger)
        {
            if (input != null)
            {
                foreach (var document in input)
                {
                    logger.LogInformation(document.id + " changed");
                    var values = new NameValueEntry[document.values.Count];
                    int i = 0;
                    foreach (KeyValuePair<string, string> entry in document.values)
                    {
                        values[i++] = new NameValueEntry(entry.Key, entry.Value);
                    }

                    await redisDB.Value.StreamAddAsync(stream, values);
                }
            }
        }
     }
}
