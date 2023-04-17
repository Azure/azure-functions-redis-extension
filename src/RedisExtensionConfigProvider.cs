﻿using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the extension context.
    /// </summary>
    [Extension("Redis")]
    public class RedisExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration configuration;
        private readonly INameResolver nameResolver;
        private readonly ILogger logger;

        /// <summary>
        /// Adds Redis triggers and bindings to the extension context.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="nameResolver"></param>
        /// <param name="logger"></param>
        public RedisExtensionConfigProvider(IConfiguration configuration, INameResolver nameResolver, ILogger logger)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.logger = logger;
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
            FluentBindingRule<RedisPubSubTriggerAttribute> pubsubTriggerRule = context.AddBindingRule<RedisPubSubTriggerAttribute>();
            pubsubTriggerRule.BindToTrigger(new RedisPubSubTriggerBindingProvider(configuration, nameResolver, logger));

            FluentBindingRule<RedisListTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListTriggerAttribute>();
            listsTriggerRule.BindToTrigger(new RedisListTriggerBindingProvider(configuration, nameResolver, logger));

            FluentBindingRule<RedisStreamTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamTriggerAttribute>();
            streamsTriggerRule.BindToTrigger(new RedisStreamTriggerBindingProvider(configuration, nameResolver, logger));
#pragma warning restore CS0618
        }
    }
}
