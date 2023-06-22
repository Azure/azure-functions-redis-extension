using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class StreamTrigger
    {
        private static CosmosClient client = new(
            connectionString: Environment.GetEnvironmentVariable("COSMOS_CONNECTION_STRING")!
        );
        public const string localhostSetting = "REDIS_CONNECTION";


        [FunctionName(nameof(StreamTriggerAsync))]
        public static async Task StreamTriggerAsync(
                [RedisStreamTrigger(localhostSetting, "streamTest")] string entry,
                ILogger logger)
        {
            //parsing information
            char[] delimiterChars = { '"', ',', '{', ':', '}' };
            string[] words = entry.Split(delimiterChars);
            words = words.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            string[] vals = new string[words.Length - 3];
            Array.Copy(words, 3, vals, 0, vals.Length);

            //retreiving the database and container
            Cosmos.Database database = await client.CreateDatabaseIfNotExistsAsync(
                id: "database-id"
            );

            Cosmos.Container container = await database.CreateContainerIfNotExistsAsync(
                id: "container-id",
                partitionKeyPath: "/category",
                throughput: 400
            );

            //formatting information
            Product item = new(
                id: words[1],
                values: vals
            );

            //upload information into cosmos db
            Product upsertedItem = await container.UpsertItemAsync<Product>(item);
        }
    }
}

public record Product(
    string id,
    string[] values
);


