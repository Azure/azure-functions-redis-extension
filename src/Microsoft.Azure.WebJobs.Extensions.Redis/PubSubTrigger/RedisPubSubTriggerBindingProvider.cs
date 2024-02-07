using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Provides trigger binding, variables configured in local.settings.json are being retrieved here.
    /// </summary>
    internal class RedisPubSubTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly AzureComponentFactory azureComponentFactory;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisPubSubTriggerBindingProvider(IConfiguration configuration, AzureComponentFactory azureComponentFactory, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.azureComponentFactory = azureComponentFactory;
            this.nameResolver = nameResolver;
            this.logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisPubSubTriggerBindingProvider)}] Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            RedisPubSubTriggerAttribute attribute = parameter.GetCustomAttribute<RedisPubSubTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                logger?.LogError($"[{nameof(RedisPubSubTriggerBindingProvider)}] No {nameof(RedisPubSubTriggerAttribute)} found in parameter of provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string channel = RedisUtilities.ResolveString(nameResolver, attribute.Channel, nameof(attribute.Channel));
            return Task.FromResult<ITriggerBinding>(new RedisPubSubTriggerBinding(configuration, azureComponentFactory, attribute.Connection, channel, parameter.ParameterType, logger));
        }
    }
}
