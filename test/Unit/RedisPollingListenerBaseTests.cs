using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Scale;
using StackExchange.Redis;
using System;
using System.Threading;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPollingListenerBaseTests
    {
        private const string connectionString = "127.0.0.1:6379";
        private const int defaultPollingInterval = 100;
        private const int defaultMessagesPerWorker = 10;
        private const int defaultCount = 10;
        private const string keys = "a";

        private static readonly RedisPollingMetrics[] increasingMetrics = new RedisPollingMetrics[] {
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 10 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 20 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 30 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 40 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 60 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 70 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 80 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 90 },
        };

        private static readonly RedisPollingMetrics[] decreasingMetrics = new RedisPollingMetrics[] {
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 90 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 80 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 70 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 60 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 40 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 30 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 20 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 10 },
        };

        private static readonly RedisPollingMetrics[] constantMetrics = new RedisPollingMetrics[] {
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-9), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-8), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-7), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-6), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-5), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 50 },
        };

        private static readonly RedisPollingMetrics[] fewMetrics = new RedisPollingMetrics[] {
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-4), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-3), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-2), Remaining = 50 },
            new RedisPollingMetrics { Timestamp = DateTime.Now.AddSeconds(-1), Remaining = 50 },
        };


        [Fact]
        public async void StartAsync_CreatesConnectionMultiplexerAsync()
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, defaultMessagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
            await listener.StartAsync(new CancellationToken());
            Assert.NotNull(listener.multiplexer);
            Assert.Equal(connectionString, listener.multiplexer.Configuration, ignoreCase: true);
        }

        [Fact]
        public async void StopAsync_ClosesAndDisposesConnectionMultiplexerAsync()
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, defaultMessagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
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
        public void ScalingLogic_ConstantMetrics(int workerCount, int messagesPerWorker, ScaleVote expected)
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, messagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
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
        public void ScalingLogic_FewMetrics(int workerCount, int messagesPerWorker)
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, messagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = fewMetrics };
            Assert.Equal(ScaleVote.None, listener.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(3, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_DecreasingMetrics(int workerCount, int messagesPerWorker, ScaleVote expected)
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, messagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = decreasingMetrics };
            Assert.Equal(expected, listener.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(7, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_IncreasingMetrics(int workerCount, int messagesPerWorker, ScaleVote expected)
        {
            RedisPollingListenerBase listener = new RedisListsListener(connectionString, keys, defaultPollingInterval, messagesPerWorker, defaultCount, false, A.Fake<ITriggeredFunctionExecutor>());
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = increasingMetrics };
            Assert.Equal(expected, listener.GetScaleStatus(context).Vote);
        }
    }
}
