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
    internal class RedisStreamsTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;

        public RedisStreamsTriggerBindingProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            ParameterInfo parameter = context.Parameter;
            RedisStreamsTriggerAttribute attribute = parameter.GetCustomAttribute<RedisStreamsTriggerAttribute>(inherit: false);

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveString(configuration, attribute.ConnectionStringSetting, "ConnectionString");
            string keys = RedisUtilities.ResolveString(configuration, attribute.Keys, "Keys");
            int messagesPerWorker = attribute.MessagesPerWorker;
            int batchSize = attribute.BatchSize;
            TimeSpan pollingInterval = TimeSpan.FromMilliseconds(attribute.PollingIntervalInMs);
            string consumerGroup = RedisUtilities.ResolveString(configuration, attribute.ConsumerGroup, "ConsumerGroup");
            bool deleteAfterProcess = attribute.DeleteAfterProcess;

            return Task.FromResult<ITriggerBinding>(new RedisStreamsTriggerBinding(connectionString, keys, pollingInterval, messagesPerWorker, batchSize, consumerGroup, deleteAfterProcess));
        }
    }
}
