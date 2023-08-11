using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FakeItEasy.Sdk;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    [Collection("PubSubTriggerTests")]
    public class PubSubCosmosIntegrationTests
    {

        [Theory]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.SingleChannelWriteBehind), PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testValue single")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.SingleChannelWriteBehind), PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testValue multi")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.MultipleChannelWriteBehind), PubSubCosmosIntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix multi")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteBehind), PubSubCosmosIntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix all")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteBehind), "prefix" + PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testPrefix all")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteBehind), "separate", "testSeparate all")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.SingleChannelWriteThrough), PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testValue single")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.SingleChannelWriteThrough), PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testValue multi")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.MultipleChannelWriteThrough), PubSubCosmosIntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix multi")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteThrough), PubSubCosmosIntegrationTestFunctions.pubsubChannel + "suffix", "testSuffix all")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteThrough), "prefix" + PubSubCosmosIntegrationTestFunctions.pubsubChannel, "testPrefix all")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.AllChannelsWriteThrough), "separate", "testSeparate all")]
        public async void PubSubMessageWrite_SuccessfullyWritesToCosmos(string functionName, string channel, string message)
        {
            //start the function trigger and publish a message to the specified pubsub channel
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7079))
            {
                ISubscriber subscriber = multiplexer.GetSubscriber();

                subscriber.Publish(channel, message);
                await Task.Delay(TimeSpan.FromSeconds(1));

                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            };

            //Query Cosmos DB for the message that was published
            string cosmosMessage = null;
            using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
            {
                var db = client.GetContainer("DatabaseId", "PSContainerId");
                var queryable = db.GetItemLinqQueryable<PubSubData>();

                //get all entries in the container that contain the correct channel
                using FeedIterator<PubSubData> feed = queryable
                    .Where(p => p.channel == channel)
                    .OrderByDescending(p => p.timestamp)
                    .ToFeedIterator();
                var response = await feed.ReadNextAsync();
                var item = response.FirstOrDefault(defaultValue: null);
                cosmosMessage = item?.message;
            }
            //check that the message was stored in Cosmos DB as expected, then clear the container
            Assert.True(message == cosmosMessage, $"Expected \"{message}\" but got \"{cosmosMessage}\"");
            IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "PSContainerId");
        }


        [Theory]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey-1", "testValue1")]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey-2", "testValue2")]
        public async void RedisToCosmos_SuccessfullyWritesToCosmos(string functionName, string key, string value)
        { 
            string keyFromCosmos = null;
            string valueFromCosmos = null;
            //start the function trigger and set a new key/value pair in Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7079))
            {
                var redisDb = multiplexer.GetDatabase();
                await redisDb.StringSetAsync(key, value);
                await Task.Delay(TimeSpan.FromSeconds(5));

                
                using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
                {
                    var cosmosDb = client.GetContainer("DatabaseId", "ContainerId");
                    var queryable = cosmosDb.GetItemLinqQueryable<RedisData>();

                //get all entries in the container that contain the correct key
                    using FeedIterator<RedisData> feed = queryable
                        .Where(p => p.key == key)
                        .OrderByDescending(p => p.timestamp)
                        .ToFeedIterator();
                    var response = await feed.ReadNextAsync();
                    await Task.Delay(TimeSpan.FromSeconds(3));

                    var item = response.FirstOrDefault(defaultValue: null);

                    keyFromCosmos = item?.key;
                    valueFromCosmos = item?.value;
                };
                //delete the key from Redis and close the function trigger
                await redisDb.KeyDeleteAsync(key);
                functionsProcess.Kill();
            };
            //Check that the key and value stored in Cosmos DB match the key and value that were set in Redis
            Assert.True(keyFromCosmos == key, $"Expected \"{key}\" but got \"{keyFromCosmos}\"");
            Assert.True(valueFromCosmos == value, $"Expected \"{value}\" but got \"{valueFromCosmos}\"");
            //clear the Cosmos DB container
            IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "ContainerId");
        }

        [Fact]
        public async void WriteAround_SuccessfullyWritesToRedis()
        {
            //start the function trigger and add a new key value pair entry to Cosmos DB
            string functionName = nameof(PubSubCosmosIntegrationTestFunctions.WriteAroundAsync);
            using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7081))
            {
                Container cosmosContainer = client.GetContainer("DatabaseId", "ContainerId");
                await Task.Delay(TimeSpan.FromSeconds(5));

                RedisData redisData = new RedisData(
                    id: Guid.NewGuid().ToString(),
                    key: "cosmosKey",
                    value: "cosmosValue",
                    timestamp: DateTime.UtcNow
                );
                
                await cosmosContainer.CreateItemAsync(redisData);
                await Task.Delay(TimeSpan.FromSeconds(10));
                client.Dispose();
                functionsProcess.Kill();

            }

            //check that the key value pair was added to Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            {
                var redisValue = await multiplexer.GetDatabase().StringGetAsync("cosmosKey");
                await Task.Delay(TimeSpan.FromSeconds(10));
                Assert.Equal("cosmosValue", redisValue);
                //await multiplexer.GetDatabase().KeyDeleteAsync("cosmosKey");
                // await Task.Delay(TimeSpan.FromSeconds(3));
                await multiplexer.CloseAsync();
            }
            //clear the Cosmos DB container
            IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "ContainerId");
        }


        [Fact]
        public async void WriteAroundMessage_SuccessfullyPublishesToRedis()
        {
            //start the function trigger and connect to Redis and Cosmos DB
            string functionName = nameof(PubSubCosmosIntegrationTestFunctions.WriteAroundMessageAsync);
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7081))
            {
                //create a subscriber for the channel that will be published to.
                //once a message is published, this lambda function will check that the message from Cosmos DB is correct in Redis
                ISubscriber subscriber = multiplexer.GetSubscriber();
                subscriber.Subscribe("PubSubChannel", (channel, message) =>
                {
                    Assert.Equal("PubSubMessage", message);
                });

                //add a document to Cosmos DB containing the pubsub channel and message
                Container cosmosContainer = client.GetContainer("DatabaseId", "PSContainerId");
                PubSubData redisData = new PubSubData(
                    id: Guid.NewGuid().ToString(),
                    channel: "PubSubChannel",
                    message: "PubSubMessage",
                    timestamp: DateTime.UtcNow
                );

                await cosmosContainer.CreateItemAsync(redisData);
                await Task.Delay(TimeSpan.FromSeconds(5));
                client.Dispose();
                functionsProcess.Kill();
                await multiplexer.CloseAsync();
            }
            IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "PSContainerId");
        }


        [Fact]
        public async void ReadThrough_SuccessfullyWritesToRedis()
        {
            //Add a key value pair to Cosmos DB that will be read by the function trigger and written to Redis
            string functionName = nameof(PubSubCosmosIntegrationTestFunctions.ReadThroughAsync);
            using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
            {
                Container cosmosContainer = client.GetContainer("DatabaseId", "ContainerId");
                RedisData redisData = new RedisData(
                    id: Guid.NewGuid().ToString(),
                    key: "cosmosKey1",
                    value: "cosmosValue1",
                    timestamp: DateTime.UtcNow
                );
                await cosmosContainer.UpsertItemAsync(redisData);
                await Task.Delay(TimeSpan.FromSeconds(2));
                client.Dispose();
            }

            //start the function trigger and connect to Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7082))
            {
                //Attempt to get the key from Redis. If the key is found, the test fails. If the key is not found, our function trigger executes and writes the key to Redis
                var redisValue = await multiplexer.GetDatabase().StringGetAsync("cosmosKey1");
                Assert.True(redisValue.IsNull, userMessage: "Key already in Redis Cache, test failed");
                await Task.Delay(TimeSpan.FromSeconds(5));

                //check that the function trigger worked as expected and the key value pair was added to Redis
                redisValue = await multiplexer.GetDatabase().StringGetAsync("cosmosKey1");
                await Task.Delay(TimeSpan.FromSeconds(3));
                Assert.Equal("cosmosValue1", redisValue);

                //clean up the cache and stop the function trigger
                await multiplexer.GetDatabase().KeyDeleteAsync("cosmosKey1");
                await Task.Delay(TimeSpan.FromSeconds(2));
                await multiplexer.CloseAsync();
                functionsProcess.Kill();
            }
            //clear the Cosmos DB container
            IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "ContainerId");
        }

        [Fact]
        public async void ReadThrough_UnsuccessfulWhenKeyNotFoundInCosmos()
        {
            string functionName = nameof(PubSubCosmosIntegrationTestFunctions.ReadThroughAsync);

            //Dictionary to store the expected output from the function trigger
            //if the output matches, the value is decmented
            Dictionary<string, int> counts = new Dictionary<string, int>
            {
                { $"Executed '{functionName}' (Failed", 1},
                { $"ERROR: Key: \"unknownKey\" not found in Redis or Cosmos DB. Try adding the Key-Value pair to Redis or Cosmos DB.", 1},
            };

            //start the function trigger and connect to Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7080))
            {
                //subscribe to the function trigger's output stream and check that the output matches the expected output
                functionsProcess.OutputDataReceived += IntegrationTestHelpers.CounterHandlerCreator(counts);

                //Attempt to get the key from Redis. If the key is found, the test fails. If the key is not found the line above decrements the values in the dictionary
                var redisValue = await multiplexer.GetDatabase().StringGetAsync("unknownKey");
                Assert.True(redisValue.IsNull, userMessage: "Key already in Redis Cache, test failed");

                await Task.Delay(TimeSpan.FromSeconds(1));
                //check that the function trigger worked as expected and the key value pair was not added to Redis
                Assert.True(redisValue.IsNull);

                //check that the expected output was written to the function trigger's output stream
                var incorrect = counts.Where(pair => pair.Value != 0);
                Assert.False(incorrect.Any(), JsonSerializer.Serialize(incorrect));

                await multiplexer.CloseAsync();
            }
        }

        [Theory]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 10)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 100)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 1000)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 10)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 100)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 1000)]
        public async void RedisToCosmos_MultipleWritesSuccessfully(string functionName, string key, string value, int numberOfWrites)
        {
            string keyFromCosmos = null;
            string valueFromCosmos = null;
            //start the function trigger and connect to Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7072))
            {
                var redisDb = multiplexer.GetDatabase();

                //iterate through the number of writes specified in the test case and write the key value pair to Redis. Then check that the key value pair was written to Cosmos before moving on the the next itteration.
                for (int i = 1; i <= numberOfWrites; i++)
                {
                    await redisDb.StringSetAsync(key + "-" + i, value + "-" + i);

                    //query Cosmos DB for the key value pair written to Redis
                    using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
                    {
                        var cosmosDb = client.GetContainer("DatabaseId", "ContainerId");
                        var queryable = cosmosDb.GetItemLinqQueryable<RedisData>();

                        //get all entries in the container that contain the missed key
                        using FeedIterator<RedisData> feed = queryable
                            .Where(p => p.key == key + "-"+ i)
                            .OrderByDescending(p => p.timestamp)
                            .ToFeedIterator();
                        var response = await feed.ReadNextAsync();
                        //await Task.Delay(TimeSpan.FromSeconds(3));

                        var item = response.FirstOrDefault(defaultValue: null);

                        keyFromCosmos = item?.key;
                        valueFromCosmos = item?.value;
                    };
                    //check that the key value pair was written to Cosmos DB
                    Assert.True(keyFromCosmos == key + "-" + i, $"Expected \"{key + "-" + i}\" but got \"{keyFromCosmos}\"");
                    Assert.True(valueFromCosmos == value + "-" + i, $"Expected \"{value + "-" + i}\" but got \"{valueFromCosmos}\"");
                }

                //Delete all key value pairs from Redis and Cosmos DB
                for (int i = 1; i <= numberOfWrites; i++) 
                {
                    await redisDb.KeyDeleteAsync(key + "-" + i);
                }
                IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "ContainerId");
                functionsProcess.Kill();
            };
        }
        [Theory]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 10)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 100)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteThrough), "testKey1", "testValue1", 1000)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 10)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 100)]
        [InlineData(nameof(PubSubCosmosIntegrationTestFunctions.WriteBehindAsync), "testKey2", "testValue2", 1000)]
        public async void RedisToCosmos_MultipleWritesSuccessfullyV2(string functionName, string key, string value, int numberOfWrites)
        {
            string keyFromCosmos = null;
            string valueFromCosmos = null;
            //start the function trigger and connect to Redis
            using (ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.localhostSetting)))
            using (Process functionsProcess = IntegrationTestHelpers.StartFunction(functionName, 7079))
            {
                var redisDb = multiplexer.GetDatabase();

                //iterate through the number of writes specified in the test case and write all the key value pairs to Redis.
                for (int i = 1; i <= numberOfWrites; i++)
                {
                    await redisDb.StringSetAsync(key + "-" + i, value + "-" + i);

                    //await Task.Delay(TimeSpan.FromSeconds(1));
                }
                await Task.Delay(TimeSpan.FromSeconds(30));

                //itterate through each key value pair written to Redis and Query Cosmos DB for that entry
                using (CosmosClient client = new CosmosClient(RedisUtilities.ResolveConnectionString(IntegrationTestHelpers.localsettings, PubSubCosmosIntegrationTestFunctions.cosmosDbConnectionSetting)))
                {
                    var cosmosDb = client.GetContainer("DatabaseId", "ContainerId");
                    var queryable = cosmosDb.GetItemLinqQueryable<RedisData>();
                    for (int i = 1; i <= numberOfWrites; i++)
                    {
                        //get all entries in the container that contain the missed key
                        using FeedIterator<RedisData> feed = queryable
                            .Where(p => p.key == key + "-" + i)
                            .OrderByDescending(p => p.timestamp)
                            .ToFeedIterator();
                        var response = await feed.ReadNextAsync();
                        //await Task.Delay(TimeSpan.FromSeconds(2));

                        var item = response.FirstOrDefault(defaultValue: null);

                        if(item == null)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(5));

                            item = response.FirstOrDefault(defaultValue: null);
                        }
                        keyFromCosmos = item?.key;
                        valueFromCosmos = item?.value;

                        //check that the key value pair was written to Cosmos DB and that the key value pair written to Redis matches the key value pair written to Cosmos DB
                        Assert.True(keyFromCosmos == key + "-" + i, $"Expected \"{key + "-" + i}\" but got \"{keyFromCosmos}\"");
                        Assert.True(valueFromCosmos == value + "-" + i, $"Expected \"{value + "-" + i}\" but got \"{valueFromCosmos}\"");
                    }
                };

                //Delete all key value pairs from Redis and Cosmos DB
                for (int i = 1; i <= numberOfWrites; i++)
                {
                    await redisDb.KeyDeleteAsync(key + "-" + i);
                }
                IntegrationTestHelpers.ClearDataFromCosmosDb("DatabaseId", "ContainerId");
                functionsProcess.Kill();
            };
        }
    }
}
