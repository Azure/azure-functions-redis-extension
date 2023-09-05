using Microsoft.Azure.WebJobs.Host.Triggers;
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
    internal class RedisStreamTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisStreamTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.logger = logger;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisStreamTriggerBindingProvider)}] Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            RedisStreamTriggerAttribute attribute = parameter.GetCustomAttribute<RedisStreamTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                logger?.LogError($"[{nameof(RedisStreamTriggerBindingProvider)}] No {nameof(RedisStreamTriggerAttribute)} found in parameter of provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting);
            string key = RedisUtilities.ResolveString(nameResolver, attribute.Key, nameof(attribute.Key));
            int count = attribute.Count;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            bool deleteAfterProcess = attribute.DeleteAfterProcess;

            return Task.FromResult<ITriggerBinding>(new RedisStreamTriggerBinding(connectionString, key, pollingInterval, count, deleteAfterProcess, parameter.ParameterType, logger));
        }
    }
}
