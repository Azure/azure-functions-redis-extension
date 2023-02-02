using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host.Triggers;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Provides trigger binding, variables configured in local.settings.json are being retrieved here.
    /// </summary>
    internal class RedisPubSubTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;

        public RedisPubSubTriggerBindingProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            
            ParameterInfo parameter = context.Parameter;
            RedisPubSubTriggerAttribute attribute = parameter.GetCustomAttribute<RedisPubSubTriggerAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = ResolveConnectionString(attribute);
            RedisTriggerType triggerType = attribute.TriggerType;
            string trigger = ResolveTrigger(attribute);

            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(connectionString, triggerType, trigger));
        }

        /// <summary>
        /// Resolves connection string from 'CacheConnection' trigger input parameter.
        /// </summary>
        public string ResolveConnectionString(RedisPubSubTriggerAttribute attributeContext)
        {
            return ResolveString(attributeContext.ConnectionString, nameof(attributeContext.ConnectionString));
        }

        /// <summary>
        /// Resolves channel name from 'ChannelName' trigger input parameter.
        /// </summary>
        public string ResolveTrigger(RedisPubSubTriggerAttribute attributeContext) 
        {
            return ResolveString(attributeContext.Trigger, nameof(attributeContext.Trigger));
        }

        public string ResolveString(string toResolve, string error)
        {
            if (string.IsNullOrEmpty(toResolve))
            {
                throw new ArgumentNullException($"Empty {error} key");
            }

            // get string from config using input as key
            if (toResolve.StartsWith("%") && toResolve.EndsWith("%"))
            {
                string configKey = toResolve.Substring(1, toResolve.Length - 2);
                string resolvedString = configuration.GetConnectionStringOrSetting(configKey);
                if (string.IsNullOrEmpty(resolvedString))
                {
                    throw new ArgumentException($"Invalid {error} - key does not exist in the config");
                }
                return resolvedString;
            }

            return toResolve;
        }
    }
}
