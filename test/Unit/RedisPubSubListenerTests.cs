using System.Threading;
using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPubSubListenerTests
    {
        //private const string connectionString = "127.0.0.1:6379";
        //private const string channel = "channel";

        //[Fact]
        //public async void StartAsync_CreatesConnectionMultiplexer()
        //{
        //    RedisPubSubListener listener = new RedisPubSubListener(new DefaultRedisService(connectionString, NullLogger.Instance), channel, A.Fake<ITriggeredFunctionExecutor>());
        //    await listener.StartAsync(new CancellationToken());
        //    Assert.NotNull(listener.redisService);
        //    Assert.Equal(connectionString, listener.redisService.Configuration, ignoreCase: true);
        //}

        //[Fact]
        //public async void StopAsync_ClosesAndDisposesConnectionMultiplexer()
        //{
        //    RedisPubSubListener listener = new RedisPubSubListener(new DefaultRedisService(connectionString, NullLogger.Instance), channel, A.Fake<ITriggeredFunctionExecutor>());
        //    listener.multiplexer = A.Fake<IConnectionMultiplexer>();
        //    await listener.StopAsync(new CancellationToken());
        //    A.CallTo(() => listener.multiplexer.CloseAsync(A<bool>._)).MustHaveHappened();
        //    A.CallTo(() => listener.multiplexer.DisposeAsync()).MustHaveHappened();
        //}
    }
}
