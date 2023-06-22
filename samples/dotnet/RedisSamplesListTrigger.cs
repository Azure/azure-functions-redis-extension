using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using System.Collections;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class RedisSamples
    {
        public const string localhostSetting = "Redis Cache Primary Connection String";
        
        //connecting to CosmosDB
        //primary connection string from your database in "Account Endpoint here"
        static readonly string Endpoint = "CosmosDB Account Endpoint here";
        static readonly CosmosClient cc = new CosmosClient(Endpoint);
        static readonly Cosmos.Container db = cc.GetDatabase("Container Name").GetContainer("Container ID Name");

        [FunctionName(nameof(ListTriggerAsync))]
        public static async Task ListTriggerAsync(
            [RedisListTrigger(localhostSetting, "listTest")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
            string value = entry.ToString();
            Guid id = Guid.NewGuid();

            Hashtable pair = new Hashtable()
            {
                { "id", id },  // Unique identifier for the document
                { "key1", entry }
            };

            ItemResponse<Hashtable> response = await db.CreateItemAsync(pair);

            Hashtable newpair = response.Resource;
        }

        [FunctionName(nameof(pubsubtrigger))]
        public static void pubsubtrigger(
            [RedisPubSubTrigger(localhostSetting, "pubsubtest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(pubsubtriggerresolvedchannel))]
        public static void pubsubtriggerresolvedchannel(
            [RedisPubSubTrigger(localhostSetting, "%pubsubchannel%")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(keyspacetrigger))]
        public static void keyspacetrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyspace@0__:keyspacetest")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(keyeventtrigger))]
        public static void keyeventtrigger(
            [RedisPubSubTrigger(localhostSetting, "__keyevent@0__:del")] string message,
            ILogger logger)
        {
            logger.LogInformation(message);
        }

        [FunctionName(nameof(streamtrigger))]
        public static void streamtrigger(
            [RedisPubSubTrigger(localhostSetting, "streamtest")] string entry,
            ILogger logger)
        {
            logger.LogInformation(entry);
        }
    }
}