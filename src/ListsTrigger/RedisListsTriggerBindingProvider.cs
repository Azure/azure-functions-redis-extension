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
    internal class RedisListsTriggerBindingProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration configuration;

        public RedisListsTriggerBindingProvider(IConfiguration configuration)
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
            RedisListsTriggerAttribute attribute = parameter.GetCustomAttribute<RedisListsTriggerAttribute>(inherit: false); 

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string connectionString = RedisUtilities.ResolveString(configuration, attribute.ConnectionString, "ConnectionString");
            string keys = RedisUtilities.ResolveString(configuration, attribute.Keys, "Keys");
            int messagesPerWorker = RedisUtilities.ResolveInt(configuration, attribute.MessagesPerWorker, "MessagesPerWorker");
            int batchSize = RedisUtilities.ResolveInt(configuration, attribute.BatchSize, "BatchSize");
            int pollingInterval = RedisUtilities.ResolveInt(configuration, attribute.PollingInterval, "PollingInterval");
            bool listPopFromBeginning = RedisUtilities.ResolveBool(configuration, attribute.ListPopFromBeginning, "ListPopFromBeginning");

            return Task.FromResult<ITriggerBinding>(new RedisListsTriggerBinding(connectionString, pollingInterval, messagesPerWorker, keys, batchSize, listPopFromBeginning));
        }
    }
}
