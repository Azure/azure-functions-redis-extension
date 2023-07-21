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
    internal class WriteAroundTrigger
    {
        private static readonly IDatabase redisDB = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("redisConnectionString")).GetDatabase();

        // Write Around - Cosmos DB to Redis 
        [FunctionName("CosmosToRedis")]
        public static void Run(
            [CosmosDBTrigger(
                databaseName: "database-id",
                containerName: "container-id",
                Connection = "cosmosConnectionString",
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

                redisDB.StreamAdd("stream2", values);
            }
        }
    }
}

public class Data
{
    public string id { get; set; }
    public Dictionary<string, string> values { get; set; }
}