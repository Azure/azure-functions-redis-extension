using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
            AzureComponentFactory azureComponentFactory = serviceProvider.GetService<AzureComponentFactory>();
            if ((triggerMetadata.Properties != null) && (triggerMetadata.Properties.TryGetValue(nameof(AzureComponentFactory), out object value)))
            {
                // If it exists, use the AzureComponentFactory passed from the ScaleController
                azureComponentFactory = value as AzureComponentFactory;
            }

            RedisPollingTriggerMetadata metadata = JsonConvert.DeserializeObject<RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());
            string key = RedisUtilities.ResolveString(nameResolver, metadata.key, nameof(metadata.key));

            if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisListTrigger, StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisListTriggerScaleMonitor(triggerMetadata.FunctionName, configuration, azureComponentFactory, metadata.connection, metadata.maxBatchSize, key);
            }
            else if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisStreamTrigger, StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisStreamTriggerScaleMonitor(triggerMetadata.FunctionName, configuration, azureComponentFactory, metadata.connection, metadata.maxBatchSize, key);
            }
            else
            {
                throw new ArgumentException($"Trigger is not the {RedisUtilities.RedisStreamTrigger} or the {RedisUtilities.RedisListTrigger}");
            }
        }

        public static string GetFunctionScalerId(string name, string type, string key) => $"{name}-{type}-{key}";

        public class RedisPollingTriggerMetadata
        {
            [JsonProperty]
            public string connection { get; set; }

            [JsonProperty]
            public string key { get; set; }

            [JsonProperty]
            public int maxBatchSize { get; set; }
        }
    }
}
