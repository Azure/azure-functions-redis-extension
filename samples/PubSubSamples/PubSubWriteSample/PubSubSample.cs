using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Azure.WebJobs.Extensions.Redis;
using StackExchange.Redis;
using System.Configuration;

namespace PubSubDemo
{
    public class PubSubSample
    {

        public const string localhostSetting = "redisLocalhost";
        public const string cosmosDbConnectionSetting = "CosmosDBConnection";

        private static readonly IDatabaseAsync s_redisDb =
            ConnectionMultiplexer.ConnectAsync(Environment.GetEnvironmentVariable(localhostSetting)).Result.GetDatabase();


        //Pub/sub Write-Behind: writes pub sub messages from Redis to CosmosDB
        [FunctionName(nameof(WritePubSubMessageToCosmosAsync))]
        public static async Task WritePubSubMessageToCosmosAsync(
            [RedisPubSubTrigger(localhostSetting, "PubSubChannel")] ChannelMessage pubSubMessage,
             [CosmosDB(
                databaseName: "DatabaseId",
                containerName: "ContainerId",
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

            //write the PubSubData object to Cosmos
            await cosmosOut.AddAsync(redisData);
            logger.LogInformation($"message: \"{redisData.message}\" from channel: \"{redisData.channel}\" stored in cosmos container: \"{"ContainerId"}\" with id: \"{redisData.id}\"");
        }
    }
}
