using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class CosmosToRedis
    {
        private static readonly IDatabase redisDB = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION")).GetDatabase();

        // Write Around - Cosmos DB to Redis 
        [FunctionName("CosmosToRedis")]
        public static void Run(
            [CosmosDBTrigger(
                databaseName: "database-id",
                containerName: "container-id",
                Connection = "COSMOS_CONNECTION",
                LeaseContainerName = "leases",
                CreateLeaseContainerIfNotExists = true)]IReadOnlyList<Data> input, ILogger logger)
        {
            foreach (var document in input)
            {
                var values = new NameValueEntry[document.values.Count];
                int i = 0;
                foreach (KeyValuePair<string, string> entry in document.values)
                {
                    values[i++] = new NameValueEntry(entry.Key, entry.Value);
                }

                redisDB.StreamAddAsync("streamTest", values);
            }
        }
    }
}

public class Data
{
    public string id { get; set; }
    public Dictionary<string, string> values { get; set; }
}


