using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisListsTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisListsTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
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

            RedisListsTriggerAttribute attribute = context.Parameter.GetCustomAttribute<RedisListsTriggerAttribute>(inherit: false);
            if (attribute is null)
            {
                logger?.LogCritical($"No {nameof(RedisListsTriggerAttribute)} found in the provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting, logger);
            string keys = RedisUtilities.ResolveString(nameResolver, attribute.Keys, nameof(attribute.Keys), logger);
            int messagesPerWorker = attribute.MessagesPerWorker;
            int batchSize = attribute.BatchSize;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            bool listPopFromBeginning = attribute.ListPopFromBeginning;

            return Task.FromResult<ITriggerBinding>(new RedisListsTriggerBinding(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, listPopFromBeginning, logger));
        }
    }
}
