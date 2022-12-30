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

            string connectionString = attribute.ConnectionString;
            int pollingInterval = attribute.PollingInterval;
            int messagesPerWorker = attribute.MessagesPerWorker;
            int count = attribute.Count;
            string keys = attribute.Keys;
            bool listPopFromBeginning = attribute.ListPopFromBeginning;

            return Task.FromResult<ITriggerBinding>(new RedisListsTriggerBinding(connectionString, pollingInterval, messagesPerWorker, keys, count, listPopFromBeginning));
        }
    }
}
