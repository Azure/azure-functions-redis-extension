using Microsoft.Azure.WebJobs.Host.Scale;
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
            long remaining = 0;
            try
            {
                IConnectionMultiplexer multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, azureComponentFactory, connection, nameof(RedisListTriggerScaleMonitor));
                remaining = await multiplexer.GetDatabase().ListLengthAsync(key);
            }
            catch (ObjectDisposedException)
            {
                // If multiplexer is disposed, then listener has already been stopped.
            }

            return new RedisPollingTriggerBaseMetrics
            {
                Remaining = remaining,
                Timestamp = DateTime.UtcNow,
            };
        }
    }
}
