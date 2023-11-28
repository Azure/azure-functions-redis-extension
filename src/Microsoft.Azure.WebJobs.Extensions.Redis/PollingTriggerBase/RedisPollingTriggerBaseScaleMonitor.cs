﻿using Microsoft.Azure.WebJobs.Host.Scale;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal abstract class RedisPollingTriggerBaseScaleMonitor: IScaleMonitor, IScaleMonitor<RedisPollingTriggerBaseMetrics>, ITargetScaler
    {
        private const int MINIMUM_SAMPLES = 5;

        internal IConnectionMultiplexer multiplexer;
        internal int maxBatchSize;
        internal string key;

        public RedisPollingTriggerBaseScaleMonitor(IConnectionMultiplexer multiplexer, int maxBatchSize, string key) 
        {
            this.multiplexer = multiplexer;
            this.maxBatchSize = maxBatchSize;
            this.key = key;
        }
        
        public ScaleMonitorDescriptor Descriptor { get; internal set; }
        public TargetScalerDescriptor TargetScalerDescriptor { get; internal set; }

        async Task<ScaleMetrics> IScaleMonitor.GetMetricsAsync()
        {
            return await this.GetMetricsAsync().ConfigureAwait(false);
        }

        public abstract Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync();

        public ScaleStatus GetScaleStatus(ScaleStatusContext<RedisPollingTriggerBaseMetrics> context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.ToArray());
        }

        public ScaleStatus GetScaleStatus(ScaleStatusContext context)
        {
            return GetScaleStatusCore(context.WorkerCount, context.Metrics?.Cast<RedisPollingTriggerBaseMetrics>().ToArray());
        }

        private ScaleStatus GetScaleStatusCore(int workerCount, RedisPollingTriggerBaseMetrics[] metrics)
        {
            // don't scale up or down if we don't have enough metrics
            if (metrics is null || metrics.Length < MINIMUM_SAMPLES)
            {
                return new ScaleStatus { Vote = ScaleVote.None };
            }

            double average = metrics.OrderByDescending(metric => metric.Timestamp).Take(MINIMUM_SAMPLES).Select(metric => metric.Remaining).Average();

            if (workerCount * maxBatchSize < average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleOut };
            }

            if ((workerCount - 1) * maxBatchSize > average)
            {
                return new ScaleStatus { Vote = ScaleVote.ScaleIn };
            }

            return new ScaleStatus { Vote = ScaleVote.None };
        }

        public async Task<TargetScalerResult> GetScaleResultAsync(TargetScalerContext context)
        {
            RedisPollingTriggerBaseMetrics metric = await GetMetricsAsync();
            return new TargetScalerResult() { TargetWorkerCount = (int)Math.Ceiling(metric.Remaining / (decimal)maxBatchSize) };
        }
    }
}
