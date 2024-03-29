﻿using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisListTriggerScaleMonitor : RedisPollingTriggerBaseScaleMonitor
    {
        public RedisListTriggerScaleMonitor(
            string name,
            IConfiguration configuration,
            AzureComponentFactory azureComponentFactory,
            string connection,
            int maxBatchSize,
            string key) 
            : base(name, configuration, azureComponentFactory, connection, maxBatchSize, key)
        {
            this.Descriptor = new ScaleMonitorDescriptor(name, RedisScalerProvider.GetFunctionScalerId(name, RedisUtilities.RedisListTrigger, key));
            this.TargetScalerDescriptor = new TargetScalerDescriptor(RedisScalerProvider.GetFunctionScalerId(name, RedisUtilities.RedisListTrigger, key));
        }

        public override async Task<RedisPollingTriggerBaseMetrics> GetMetricsAsync()
        {
            IConnectionMultiplexer multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, azureComponentFactory, connection, nameof(RedisListTriggerScaleMonitor));
            var metrics = new RedisPollingTriggerBaseMetrics
            {
                Remaining = await multiplexer.GetDatabase().ListLengthAsync(key),
                Timestamp = DateTime.UtcNow,
            };

            return metrics;
        }
    }
}
