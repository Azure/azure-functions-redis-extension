using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPollingListenerBaseTests
    {
        private const string name = nameof(RedisPollingListenerBaseTests);
        private const string connectionString = "127.0.0.1:6379";
        private const int defaultBatchSize = 10;
        private const string key = "a";
        private TimeSpan defaultPollingInterval = TimeSpan.FromMilliseconds(100);

        private static readonly RedisPollingTriggerBaseMetrics[] increasingMetrics = new RedisPollingTriggerBaseMetrics[] {
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 10 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 20 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 30 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 40 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 60 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 70 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 80 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 90 },
        };

        private static readonly RedisPollingTriggerBaseMetrics[] decreasingMetrics = new RedisPollingTriggerBaseMetrics[] {
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 90 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 80 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 70 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 60 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 40 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 30 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 20 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 10 },
        };

        private static readonly RedisPollingTriggerBaseMetrics[] constantMetrics = new RedisPollingTriggerBaseMetrics[] {
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 50 },
        };

        private static readonly RedisPollingTriggerBaseMetrics[] fewMetrics = new RedisPollingTriggerBaseMetrics[] {
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 50 },
            new RedisPollingTriggerBaseMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 50 },
        };


        [Fact]
        public async void StartAsync_CreatesConnectionMultiplexerAsync()
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, defaultBatchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            await listener.StartAsync(new CancellationToken());
            Assert.NotNull(listener.multiplexer);
            Assert.Equal(connectionString, listener.multiplexer.Configuration, ignoreCase: true);
        }

        [Fact]
        public async void StopAsync_ClosesAndDisposesConnectionMultiplexerAsync()
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, defaultBatchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            listener.multiplexer = A.Fake<IConnectionMultiplexer>();
            await listener.StopAsync(new CancellationToken());
            A.CallTo(() => listener.multiplexer.CloseAsync(A<bool>._)).MustHaveHappened();
            A.CallTo(() => listener.multiplexer.DisposeAsync()).MustHaveHappened();
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(5, 5, ScaleVote.ScaleOut)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(5, 10, ScaleVote.None)]
        [InlineData(3, 30, ScaleVote.ScaleIn)]
        [InlineData(5, 20, ScaleVote.ScaleIn)]
        public void ScalingLogic_ConstantMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, batchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = constantMetrics };
            Assert.Equal(expected, listener.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(5, 5)]
        [InlineData(1, 100)]
        [InlineData(5, 10)]
        [InlineData(3, 30)]
        [InlineData(5, 20)]
        public void ScalingLogic_FewMetrics(int workerCount, int batchSize)
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, batchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = fewMetrics };
            Assert.Equal(ScaleVote.None, listener.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(3, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_DecreasingMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, batchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = decreasingMetrics };
            Assert.Equal(expected, listener.GetScaleStatus(context).Vote);
        }
        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(7, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_IncreasingMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, batchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = increasingMetrics };
            Assert.Equal(expected, listener.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(10, 10, 1)]
        [InlineData(50, 10, 5)]
        [InlineData(100, 10, 10)]
        [InlineData(500, 10, 50)]
        [InlineData(1000, 50, 20)]
        [InlineData(5000, 100, 50)]
        [InlineData(10000, 10, 1000)]
        public async Task TargetScaler_IncreasingMetricsAsync(long remaining, int batchSize, int expected)
        {
            RedisPollingTriggerBaseListener listener = new RedisListListener(name, connectionString, key, defaultPollingInterval, batchSize, false, true, A.Fake<ITriggeredFunctionExecutor>(), A.Fake<ILogger>());
            IDatabase fakeDatabase = A.Fake<IDatabase>();
            A.CallTo(() => fakeDatabase.ListLength(A<RedisKey>._, A<CommandFlags>._)).Returns(remaining);
            IConnectionMultiplexer fakeMultiplexer = A.Fake<IConnectionMultiplexer>();
            A.CallTo(() => fakeMultiplexer.GetDatabase(A<int>._, A<object>._)).Returns(fakeDatabase);
            listener.multiplexer = fakeMultiplexer;
            Assert.Equal(expected, (await listener.GetScaleResultAsync(null)).TargetWorkerCount);
        }
    }
}
