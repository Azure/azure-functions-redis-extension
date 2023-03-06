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
                logger?.LogError($"[{nameof(RedisStreamsTriggerBindingProvider)}] Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            ParameterInfo parameter = context.Parameter;
            RedisStreamsTriggerAttribute attribute = parameter.GetCustomAttribute<RedisStreamsTriggerAttribute>(inherit: false);

            if (attribute is null)
            {
                logger?.LogError($"[{nameof(RedisStreamsTriggerBindingProvider)}] No {nameof(RedisStreamsTriggerAttribute)} found in parameter of provided {nameof(TriggerBindingProviderContext)}.");
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveConnectionString(configuration, attribute.ConnectionStringSetting);
            string keys = RedisUtilities.ResolveString(nameResolver, attribute.Keys, nameof(attribute.Keys));
            int messagesPerWorker = attribute.MessagesPerWorker;
            int batchSize = attribute.BatchSize;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            string consumerGroup = RedisUtilities.ResolveString(nameResolver, attribute.ConsumerGroup, nameof(attribute.ConsumerGroup));
            bool deleteAfterProcess = attribute.DeleteAfterProcess;

            return Task.FromResult<ITriggerBinding>(new RedisStreamsTriggerBinding(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, consumerGroup, deleteAfterProcess, logger));
        }
    }
}
