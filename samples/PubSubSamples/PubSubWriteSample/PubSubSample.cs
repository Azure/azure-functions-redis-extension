using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Redis;
using StackExchange.Redis;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;

namespace PubSubDemo
{
    public class PubSubSample
    {
        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";


        //Pub/sub Write-Behind: writes pub sub messages from Redis to Cosmos DB
        [FunctionName(nameof(WritePubSubMessageToCosmosAsync))]
        public static async Task WritePubSubMessageToCosmosAsync(
            [RedisPubSubTrigger(localhostSetting, "PubSubChannel")] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "PSContainerId",
                Connection = cosmosDbConnectionSetting)]IAsyncCollector<PubSubData> cosmosOut,
            ILogger logger)
        {
            //create a PubSubData object from the pubsub message
            PubSubData redisData = new PubSubData(
                id: Guid.NewGuid().ToString(),
                channel: pubSubMessage.Channel,
                message: pubSubMessage.Message,
                timestamp: DateTime.UtcNow
                );

            //write the PubSubData object to Cosmos DB
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"Message: \"{redisData.message}\" from Channel: \"{redisData.channel}\" stored in Cosmos DB container: \"{"PSContainerId"}\" with id: \"{redisData.id}\"");
        }
    }
}
