using System.Threading;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPubSubListenerTests
    {
        private const string connectionString = "127.0.0.1:6379";
        private const string channel = "channel";

        [Fact]
        public async void StartAsync_CreatesConnectionMultiplexer()
        {
            RedisPubSubListener listener = new RedisPubSubListener(connectionString, channel, A.Fake<ITriggeredFunctionExecutor>());
            await listener.StartAsync(new CancellationToken());
            Assert.NotNull(listener.multiplexer);
            Assert.Equal(connectionString, listener.multiplexer.Configuration, ignoreCase: true);
        }

        [Fact]
        public async void StopAsync_ClosesAndDisposesConnectionMultiplexer()
        {
            RedisPubSubListener listener = new RedisPubSubListener(connectionString, channel, A.Fake<ITriggeredFunctionExecutor>());
            listener.multiplexer = A.Fake<IConnectionMultiplexer>();
            await listener.StopAsync(new CancellationToken());
            A.CallTo(() => listener.multiplexer.CloseAsync(A<bool>._)).MustHaveHappened();
            A.CallTo(() => listener.multiplexer.DisposeAsync()).MustHaveHappened();
        }
    }
}
