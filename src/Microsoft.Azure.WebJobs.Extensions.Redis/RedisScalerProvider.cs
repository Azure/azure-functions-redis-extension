using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisScalerProvider : IScaleMonitorProvider, ITargetScalerProvider
    {
        private readonly RedisPollingTriggerBaseScaleMonitor scaleMonitor;

        public IScaleMonitor GetMonitor()
        {
            return scaleMonitor;
        }

        public ITargetScaler GetTargetScaler()
        {
            return scaleMonitor;
        }

        public RedisScalerProvider(IServiceProvider serviceProvider, TriggerMetadata triggerMetadata)
        {
            IConfiguration configuration = serviceProvider.GetService<IConfiguration>();
            INameResolver nameResolver = serviceProvider.GetService<INameResolver>();

            RedisPollingTriggerBaseAttribute attribute = JsonConvert.DeserializeObject<RedisPollingTriggerBaseAttribute>(triggerMetadata.Metadata.ToString());
            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting);
            string key = RedisUtilities.ResolveString(nameResolver, attribute.Key, nameof(attribute.Key));
            int maxBatchSize = attribute.MaxBatchSize;
            IConnectionMultiplexer multiplexer = RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexer(configuration, connectionString);

            if (string.Equals(triggerMetadata.Type, "redisListTrigger", StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisListTriggerScaleMonitor(multiplexer, triggerMetadata.FunctionName, maxBatchSize, key);
            }
            else if (string.Equals(triggerMetadata.Type, "redisStreamTrigger", StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisStreamTriggerScaleMonitor(multiplexer, triggerMetadata.FunctionName, maxBatchSize, key);
            }
            else
            {
                throw new ArgumentException("Trigger is not the RedisStreamTrigger or the RedisListTrigger");
            }
        }
    }
}
