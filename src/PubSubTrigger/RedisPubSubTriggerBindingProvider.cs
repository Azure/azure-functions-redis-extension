using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisPubSubTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisPubSubTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                logger?.LogCritical($"Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException("context");
            }
            
            RedisPubSubTriggerAttribute attribute = context.Parameter.GetCustomAttribute<RedisPubSubTriggerAttribute>(inherit: false);
            if (attribute is null)
            {
                logger?.LogCritical($"No {nameof(RedisPubSubTriggerAttribute)} found in the provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting, logger);
            string channel = RedisUtilities.ResolveString(nameResolver, attribute.Channel, nameof(attribute.Channel), logger);

            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(connectionString, channel, logger));
        }
    }
}
