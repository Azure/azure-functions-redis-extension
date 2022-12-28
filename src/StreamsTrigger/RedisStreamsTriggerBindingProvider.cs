﻿using System;
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

            string connectionString = attribute.ConnectionString;
            string keys = attribute.Keys;
            string consumerGroup = attribute.ConsumerGroup;
            string consumerName = attribute.ConsumerName;
            int count = attribute.Count;
            int pollingInterval = attribute.PollingInterval;

            return Task.FromResult<ITriggerBinding>(new RedisStreamsTriggerBinding(connectionString, keys, consumerGroup, consumerName, count, pollingInterval));
        }
    }
}
