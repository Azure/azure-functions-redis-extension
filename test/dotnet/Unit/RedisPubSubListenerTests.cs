using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Threading;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPubSubListenerTests
    {
        [Fact]
        public async void StopAsync_ClosesAndDisposesConnectionMultiplexer()
        {
            IConnectionMultiplexer multiplexer = A.Fake<IConnectionMultiplexer>();
            RedisPubSubListener listener = new RedisPubSubListener("name", multiplexer, "channel", false, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            await listener.StopAsync(new CancellationToken());
            A.CallTo(() => listener.multiplexer.CloseAsync(A<bool>._)).MustHaveHappened();
            A.CallTo(() => listener.multiplexer.DisposeAsync()).MustHaveHappened();
        }
    }
}
