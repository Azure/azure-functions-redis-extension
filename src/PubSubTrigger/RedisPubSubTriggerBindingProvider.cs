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
        private readonly INameResolver nameResolver;

        public RedisPubSubTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
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

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting);
            string channel = RedisUtilities.ResolveString(nameResolver, attribute.Channel, nameof(attribute.Channel));

            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(connectionString, channel));
        }
    }
}
