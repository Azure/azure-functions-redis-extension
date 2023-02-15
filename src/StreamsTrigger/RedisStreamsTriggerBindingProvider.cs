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
    internal class RedisStreamsTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        public RedisStreamsTriggerBindingProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
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

            RedisStreamsTriggerAttribute attribute = context.Parameter.GetCustomAttribute<RedisStreamsTriggerAttribute>(inherit: false);
            if (attribute is null)
            {
                logger?.LogCritical($"No {nameof(RedisStreamsTriggerAttribute)} found in the provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting, logger);
            string keys = RedisUtilities.ResolveString(nameResolver, attribute.Keys, nameof(attribute.Keys), logger);
            int messagesPerWorker = attribute.MessagesPerWorker;
            int batchSize = attribute.BatchSize;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            string consumerGroup = RedisUtilities.ResolveString(nameResolver, attribute.ConsumerGroup, nameof(attribute.ConsumerGroup), logger);
            bool deleteAfterProcess = attribute.DeleteAfterProcess;

            return Task.FromResult<ITriggerBinding>(new RedisStreamsTriggerBinding(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, consumerGroup, deleteAfterProcess, logger));
        }
    }
}
