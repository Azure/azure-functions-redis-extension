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

            string connectionString = RedisUtilities.ResolveString(configuration, attribute.ConnectionString, "ConnectionString");
            RedisTriggerType triggerType = RedisUtilities.ResolveTriggerType(configuration, attribute.TriggerType, "TriggerType");
            string trigger = RedisUtilities.ResolveString(configuration, attribute.Trigger, "Trigger");

            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(connectionString, triggerType, trigger));
        }
    }
}
