using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using StackExchange.Redis;
using System.Threading;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPubSubListenerTests
    {
        private const string connectionString = "127.0.0.1:6379";
        private const string trigger = "trigger";

        [Fact]
        public async void StartAsync_CreatesConnectionMultiplexerAsync()
        {
            RedisPubSubListener listener = new RedisPubSubListener(connectionString, RedisTriggerType.PubSub, trigger, A.Fake<ITriggeredFunctionExecutor>());
            await listener.StartAsync(new CancellationToken());
            Assert.NotNull(listener.multiplexer);
            Assert.Equal(connectionString, listener.multiplexer.Configuration, ignoreCase: true);
        }

        [Fact]
        public async void StopAsync_ClosesAndDisposesConnectionMultiplexer()
        {
            RedisPubSubListener listener = new RedisPubSubListener(connectionString, RedisTriggerType.PubSub, trigger, A.Fake<ITriggeredFunctionExecutor>());
            listener.multiplexer = A.Fake<IConnectionMultiplexer>();
            await listener.StopAsync(new CancellationToken());
            A.CallTo(() => listener.multiplexer.Close(A<bool>._)).MustHaveHappened();
            A.CallTo(() => listener.multiplexer.Dispose()).MustHaveHappened();
        }
    }
}
