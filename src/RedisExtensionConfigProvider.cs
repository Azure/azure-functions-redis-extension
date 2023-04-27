using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text.Json;

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
        private readonly ILoggerFactory loggerFactory;

        /// <summary>
        /// Adds Redis triggers and bindings to the extension context.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="nameResolver"></param>
        /// <param name="loggerFactory"></param>
        public RedisExtensionConfigProvider(IConfiguration configuration, INameResolver nameResolver, ILoggerFactory loggerFactory)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
            this.loggerFactory = loggerFactory;
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
            pubsubTriggerRule.BindToTrigger(new RedisPubSubTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger("RedisPubSubTrigger")));

            FluentBindingRule<RedisListTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListTriggerAttribute>();
            listsTriggerRule.BindToTrigger(new RedisListTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger("RedisListTrigger")));

            FluentBindingRule<RedisStreamTriggerAttribute> streamTriggerRule = context.AddBindingRule<RedisStreamTriggerAttribute>();
            streamTriggerRule.BindToTrigger(new RedisStreamTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger("RedisStreamTrigger")));
#pragma warning restore CS0618
        }
    }
}
