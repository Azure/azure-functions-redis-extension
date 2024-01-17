using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.WriteAround
{
    public static class PubSubWriteAround
    {
        //Connection string settings that will be resolved from local.settings.json file
        public const string RedisConnectionSetting = "RedisConnectionString";
        public const string CosmosDbConnectionSetting = "CosmosDbConnectionString";

        //Cosmos DB settings that will be resolved from local.settings.json file
        public const string DatabaseSetting = "%CosmosDbDatabaseId%";
        public const string ContainerSetting = "%PubSubCosmosDbContainerId%";
        public const string PubSubContainerSetting = "%MessagesCosmosDbContainerId%";

        private static readonly Lazy<IConnectionMultiplexer> s_redisConnection = new Lazy<IConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable(RedisConnectionSetting)));

        /// <summary>
        /// Triggers when pubsub messages are written directly to Cosmos DB, then asynchronously publishes the associated message to the Redis channel that was specified.
        /// </summary>
        /// <param name="cosmosData"> A readonly list containing all the new/updated documents in the specified Cosmos DB container.</param>
        /// <param name="logger"> An ILogger that is used to write informational log messages.</param>
        /// <returns></returns>
        [FunctionName(nameof(PubSubWriteAround))]
        public static async Task Run(
            [CosmosDBTrigger(
                databaseName: DatabaseSetting,
                containerName: PubSubContainerSetting,
                Connection = CosmosDbConnectionSetting,
                LeaseContainerName = "leases", LeaseContainerPrefix = "Write-Around-")]IReadOnlyList<PubSubData> cosmosData,
            ILogger logger)
        {
            //if the list is null or empty, return
            if (cosmosData == null || cosmosData.Count <= 0) return;

            ISubscriber redisPublisher = s_redisConnection.Value.GetSubscriber();
            //for each new item upladed to cosmos, publish to Redis
            foreach (var document in cosmosData)
            {
                //publish the message to the correct Redis channel
                await redisPublisher.PublishAsync(document.channel, document.message);
                logger.LogInformation($"Message: \"{document.message}\" has been published on channel: \"{document.channel}\".");
            }
        }
    }
}
