using Microsoft.Azure.WebJobs.Host.Scale;
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

            RedisPollingTriggerMetadata metadata = JsonConvert.DeserializeObject<RedisPollingTriggerMetadata>(triggerMetadata.Metadata.ToString());
            string key = RedisUtilities.ResolveString(nameResolver, metadata.key, nameof(metadata.key));

            if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisListTrigger, StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisListTriggerScaleMonitor(triggerMetadata.FunctionName, configuration, metadata.connectionStringSetting, metadata.maxBatchSize, key);
            }
            else if (string.Equals(triggerMetadata.Type, RedisUtilities.RedisStreamTrigger, StringComparison.OrdinalIgnoreCase))
            {
                scaleMonitor = new RedisStreamTriggerScaleMonitor(triggerMetadata.FunctionName, configuration, metadata.connectionStringSetting, metadata.maxBatchSize, key);
            }
            else
            {
                throw new ArgumentException("Trigger is not the RedisStreamTrigger or the RedisListTrigger");
            }
        }

        public static string GetFunctionScalerId(string name, string type, string key) => $"{name}-{type}-{key}";

        public class RedisPollingTriggerMetadata
        {
            [JsonProperty]
            public string connectionStringSetting { get; set; }

            [JsonProperty]
            public string key { get; set; }

            [JsonProperty]
            public int maxBatchSize { get; set; }
        }
    }
}
