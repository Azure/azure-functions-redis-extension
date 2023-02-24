using System;
using System.Collections.Concurrent;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the extension context.
    /// </summary>
    [Extension("Redis")]
    public class RedisExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<string, IRedisService> connections;

        /// <summary>
        ///
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public RedisExtensionConfigProvider(IConfiguration configuration, ILogger logger)
        {
            this.configuration = configuration;
            this.logger = logger;

            connections = new ConcurrentDictionary<string, IRedisService>();
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
            pubsubTriggerRule.BindToTrigger<RedisMessageModel>(new RedisPubSubTriggerBindingProvider(configuration, logger, this));

            FluentBindingRule<RedisListsTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListsTriggerAttribute>();
            listsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisListsTriggerBindingProvider(configuration));

            FluentBindingRule<RedisStreamsTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamsTriggerAttribute>();
            streamsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisStreamsTriggerBindingProvider(configuration));
#pragma warning restore CS0618
        }

        internal IRedisService GetService(string connectionString, ILogger logger)
        {
            return connections.GetOrAdd(connectionString, (cs) => new CountingRedisService(connectionString, logger));
        }

    }
}
