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
            listsTriggerRule.BindToTrigger<RedisListEntry>(new RedisListTriggerBindingProvider(configuration, nameResolver, logger));
            listsTriggerRule.AddConverter<RedisListEntry, string>(listEntry => listEntry.Value);

            FluentBindingRule<RedisStreamTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamTriggerAttribute>();
            streamsTriggerRule.BindToTrigger<RedisStreamEntry>(new RedisStreamTriggerBindingProvider(configuration, nameResolver, logger));
            streamsTriggerRule.AddConverter<RedisStreamEntry, KeyValuePair<string, string>[]>(entry => entry.Values);
            streamsTriggerRule.AddConverter<RedisStreamEntry, string>(entry => JsonSerializer.Serialize(entry.Values));
            streamsTriggerRule.AddConverter<RedisStreamEntry, IReadOnlyDictionary<string, string>>(entry => entry.Values.ToDictionary());
#pragma warning restore CS0618
        }
    }
}
