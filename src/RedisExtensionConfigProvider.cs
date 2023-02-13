﻿using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the extension context.
    /// </summary>
    [Extension("Redis")]
    public class RedisExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration configuration;
        /// <summary>
        /// Adds Redis triggers and bindings to the extension context.
        /// </summary>
        /// <param name="configuration"></param>
        public RedisExtensionConfigProvider(IConfiguration configuration) 
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Initializes triggers and bindings via binding rule.
        /// </summary>
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

#pragma warning disable CS0618
            FluentBindingRule<RedisPubSubTriggerAttribute> rule = context.AddBindingRule<RedisPubSubTriggerAttribute>();
#pragma warning restore CS0618
            rule.BindToTrigger<RedisMessageModel>(new RedisPubSubTriggerBindingProvider(configuration));
        }
    }
}