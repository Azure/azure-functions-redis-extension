using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;
using StackExchange.Redis;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisListenerTests
    {
        private const string connectionString = "127.0.0.1:6379,abortConnect=false";
        private const string trigger = "trigger";

        [Fact]
        public void StartAsync_CreatesConnectionMultiplexer()
        {
            RedisListener listener = new RedisListener(connectionString, RedisTriggerType.PubSub, trigger, A.Fake<ITriggeredFunctionExecutor>());
            listener.StartAsync(new CancellationToken());
            Assert.NotNull(listener.multiplexer);
            Assert.Equal(connectionString, listener.multiplexer.Configuration, ignoreCase: true);
        }

        [Fact]
        public void StopAsync_ClosesAndDisposesConnectionMultiplexer()
        {
            RedisListener listener = new RedisListener(connectionString, RedisTriggerType.PubSub, trigger, A.Fake<ITriggeredFunctionExecutor>());
            listener.multiplexer = A.Fake<IConnectionMultiplexer>();
            listener.StopAsync(new CancellationToken());
            A.CallTo(() => listener.multiplexer.Close(A<bool>._)).MustHaveHappened();
            A.CallTo(() => listener.multiplexer.Dispose()).MustHaveHappened();
        }
    }
}
