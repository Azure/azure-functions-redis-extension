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
    internal class RedisListTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly AzureComponentFactory azureComponentFactory;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisListTriggerBindingProvider(IConfiguration configuration, AzureComponentFactory azureComponentFactory, INameResolver nameResolver, ILogger logger)
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
                logger?.LogError($"[{nameof(RedisListTriggerBindingProvider)}] Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            RedisListTriggerAttribute attribute = parameter.GetCustomAttribute<RedisListTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                logger?.LogError($"[{nameof(RedisListTriggerBindingProvider)}] No {nameof(RedisListTriggerAttribute)} found in parameter of provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string key = RedisUtilities.ResolveString(nameResolver, attribute.Key, nameof(attribute.Key));
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);

            return Task.FromResult<ITriggerBinding>(new RedisListTriggerBinding(configuration, azureComponentFactory, attribute.Connection, key, pollingInterval, attribute.MaxBatchSize, attribute.ListDirection, parameter.ParameterType, logger));
        }
    }
}
