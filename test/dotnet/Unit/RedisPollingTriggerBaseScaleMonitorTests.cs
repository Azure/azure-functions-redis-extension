using FakeItEasy;
using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using Xunit;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Unit
{
    public class RedisPollingTriggerBaseScaleMonitorTests
    {
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

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(5, 5, ScaleVote.ScaleOut)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(5, 10, ScaleVote.None)]
        [InlineData(3, 30, ScaleVote.ScaleIn)]
        [InlineData(5, 20, ScaleVote.ScaleIn)]
        public void ScalingLogic_ConstantMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisListTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), "connection", batchSize, "key");
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = constantMetrics };
            Assert.Equal(expected, monitor.GetScaleStatus(context).Vote);
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
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisListTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), "connection", batchSize, "key");
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = fewMetrics };
            Assert.Equal(ScaleVote.None, monitor.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(3, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_DecreasingMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisListTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), "connection", batchSize, "key");
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = decreasingMetrics };
            Assert.Equal(expected, monitor.GetScaleStatus(context).Vote);
        }

        [Theory]
        [InlineData(1, 10, ScaleVote.ScaleOut)]
        [InlineData(7, 10, ScaleVote.None)]
        [InlineData(1, 100, ScaleVote.None)]
        [InlineData(10, 10, ScaleVote.ScaleIn)]
        public void ScalingLogic_IncreasingMetrics(int workerCount, int batchSize, ScaleVote expected)
        {
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisListTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), "connection", batchSize, "key");
            ScaleStatusContext context = new ScaleStatusContext { WorkerCount = workerCount, Metrics = increasingMetrics };
            Assert.Equal(expected, monitor.GetScaleStatus(context).Vote);
        }

        [Fact]
        public async void RedisListTriggerScaleMonitor_DoesntThrowObjectDisposedException()
        {
            string connection = "connection";
            IConnectionMultiplexer fakeMultiplexer = A.Fake<IConnectionMultiplexer>();
            RedisExtensionConfigProvider.connectionMultiplexerCache.TryAdd(connection, fakeMultiplexer);
            A.CallTo(() => fakeMultiplexer.GetDatabase(A<int>._, A<object>._)).Throws(new ObjectDisposedException("fake"));
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisListTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), connection, 1, "key");
            RedisPollingTriggerBaseMetrics metrics = await monitor.GetMetricsAsync();
            Assert.Equal(0, metrics.Remaining);
        }

        [Fact]
        public async void RedisStreamTriggerScaleMonitor_DoesntThrowObjectDisposedException()
        {
            string connection = "connection";
            IConnectionMultiplexer fakeMultiplexer = A.Fake<IConnectionMultiplexer>();
            RedisExtensionConfigProvider.connectionMultiplexerCache.TryAdd(connection, fakeMultiplexer);
            A.CallTo(() => fakeMultiplexer.GetDatabase(A<int>._, A<object>._)).Throws(new ObjectDisposedException("fake"));
            RedisPollingTriggerBaseScaleMonitor monitor = new RedisStreamTriggerScaleMonitor("name", A.Fake<IConfiguration>(), A.Fake<AzureComponentFactory>(), connection, 1, "key");
            RedisPollingTriggerBaseMetrics metrics = await monitor.GetMetricsAsync();
            Assert.Equal(0, metrics.Remaining);
        }
    }
}
