using Xunit;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;
using StackExchange.Redis;
using System.Threading;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache.Tests;


namespace XUnitFunctionTests
{
    public class AzureRedisCacheListenerTests
    {
        /* NOTE:
        Tests will NOT run unless cache connection string & channel name
        are provided in tests.local.settings.json (or your own custom config json file).
        */

        static TestCacheConfig cacheConfig = new TestCacheConfig("Integration/tests.local.settings.json");
        static string cacheConnectionString = cacheConfig.connectionString;
        static string channelName = cacheConfig.channelName;

        static CancellationToken cancellationToken = new CancellationToken();
        static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(cacheConnectionString);
        static ISubscriber subscriber = redis.GetSubscriber();
        static bool isKeySpaceNotificaitionsEnabled = false;
        AzureRedisCacheListener funcListener = new AzureRedisCacheListener(cacheConnectionString, channelName, isKeySpaceNotificaitionsEnabled);

        ///<summary>
        ///Testing if correct connection multiplexer was created.
        ///</summary>
        [Fact]
        public void ConnectionMultiplexer_Creation_Test()
        {
            funcListener.InitializeConnectionString(cacheConnectionString);
            ConnectionMultiplexer testRes = funcListener.getConnectionMultiplexer();

            Assert.Equal(redis.ClientName, testRes.ClientName);
        }

        [Fact]
        public void EnablingKeySpaceNotifications_Test()
        {
            //Setting keyspace notifications flag to true, this is slightly redundant in this test case:
            isKeySpaceNotificaitionsEnabled = true;

            var KSNListener = new AzureRedisCacheListener(cacheConnectionString, channelName, isKeySpaceNotificaitionsEnabled);
            KSNListener.InitializeConnectionString(cacheConnectionString);
            KSNListener.EnableKeyspaceNotifications(subscriber, cancellationToken);

            var subscriberState = (int)KSNListener.getSubscriberState();

            Assert.Equal(0, subscriberState);
        }

        [Fact]
        public void EnablingPubSub_Test()
        {
            //Setting keyspace notifications flag to false, aka only use pub sub:
            isKeySpaceNotificaitionsEnabled = false;

            var PSListener = new AzureRedisCacheListener(cacheConnectionString, channelName, isKeySpaceNotificaitionsEnabled);
            PSListener.InitializeConnectionString(cacheConnectionString);
            PSListener.EnablePubSub(subscriber, cancellationToken);

            var subscriberState = (int)PSListener.getSubscriberState();

            Assert.Equal(1, subscriberState);
        }

        [Fact]
        public void DefaultSubscriberState_Test()
        {
            var stopSubListener = new AzureRedisCacheListener(cacheConnectionString, channelName, isKeySpaceNotificaitionsEnabled);
            var subscriberState = (int)stopSubListener.getSubscriberState();

            Assert.Equal(2, subscriberState);
        }

        [Fact]
        public void ConnectionMultiplexer_Deletion_Test()
        {
            //Setup
            funcListener.InitializeConnectionString(cacheConnectionString);
            ConnectionMultiplexer testRes = funcListener.getConnectionMultiplexer();

            //Close multiplexers
            funcListener.CloseMultiplexer(testRes);
            redis.Close();

            Assert.Equal(redis.IsConnected, testRes.IsConnected);
        }

    }

}
