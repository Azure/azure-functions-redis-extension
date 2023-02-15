using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Provides trigger binding, variables configured in local.settings.json are being retrieved here.
    /// </summary>
    internal class RedisPubSubTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly RedisExtensionConfigProvider provider;

        public RedisPubSubTriggerBindingProvider(IConfiguration configuration, ILogger logger, RedisExtensionConfigProvider provider)
        {
            this.configuration = configuration;
            this.logger = logger;
            this.provider = provider;
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
            string channel = RedisUtilities.ResolveString(configuration, attribute.Channel, "Channel");

            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(provider.GetService(connectionString, logger), channel));
        }
    }
}
