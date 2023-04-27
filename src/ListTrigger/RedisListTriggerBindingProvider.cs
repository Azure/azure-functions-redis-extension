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
    internal class RedisListTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisListTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
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

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting);
            string keys = RedisUtilities.ResolveString(nameResolver, attribute.Keys, nameof(attribute.Keys));
            int messagesPerWorker = attribute.MessagesPerWorker;
            int count = attribute.Count;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            bool listPopFromBeginning = attribute.ListPopFromBeginning;

            return Task.FromResult<ITriggerBinding>(new RedisListTriggerBinding(connectionString, keys, pollingInterval, messagesPerWorker, count, listPopFromBeginning, logger));
        }
    }
}
