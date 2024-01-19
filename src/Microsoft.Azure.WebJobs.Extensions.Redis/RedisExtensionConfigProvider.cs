﻿using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the extension context.
    /// </summary>
    [Extension("Redis")]
    internal class RedisExtensionConfigProvider : IExtensionConfigProvider
    {
        internal readonly IConfiguration configuration;
        internal readonly INameResolver nameResolver;
        internal readonly ILoggerFactory loggerFactory;

        private static readonly ConcurrentDictionary<string, IConnectionMultiplexer> connectionMultiplexerCache = new ConcurrentDictionary<string, IConnectionMultiplexer>();

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
            pubsubTriggerRule.BindToTrigger(new RedisPubSubTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger(RedisUtilities.RedisPubSubTrigger)));

            FluentBindingRule<RedisListTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListTriggerAttribute>();
            listsTriggerRule.BindToTrigger(new RedisListTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger(RedisUtilities.RedisListTrigger)));

            FluentBindingRule<RedisStreamTriggerAttribute> streamTriggerRule = context.AddBindingRule<RedisStreamTriggerAttribute>();
            streamTriggerRule.BindToTrigger(new RedisStreamTriggerBindingProvider(configuration, nameResolver, loggerFactory.CreateLogger(RedisUtilities.RedisStreamTrigger)));

            FluentBindingRule<RedisAttribute> bindingRule = context.AddBindingRule<RedisAttribute>();
            bindingRule.BindToCollector(new RedisAsyncCollectorAsyncConverter(configuration, loggerFactory.CreateLogger(RedisUtilities.RedisOutputBinding)));
            bindingRule.BindToInput<OpenType>(typeof(RedisAsyncConverter<>), configuration, nameResolver, loggerFactory.CreateLogger(RedisUtilities.RedisInputBinding));
#pragma warning restore CS0618
        }

        internal static async Task<IConnectionMultiplexer> GetOrCreateConnectionMultiplexerAsync(IConfiguration configuration, string connectionStringSetting, string clientName = "")
        {
            if (connectionMultiplexerCache.ContainsKey(connectionStringSetting))
            {
                connectionMultiplexerCache.TryGetValue(connectionStringSetting, out IConnectionMultiplexer connectionMultiplexer);
                return connectionMultiplexer;
            }
            else
            {
                IConnectionMultiplexer connectionMultiplexer = await CreateConnectionMultiplexerAsync(configuration, connectionStringSetting, clientName);
                return connectionMultiplexerCache.GetOrAdd(connectionStringSetting, connectionMultiplexer);
            }
        }

        internal static async Task<IConnectionMultiplexer> CreateConnectionMultiplexerAsync(IConfiguration configuration, string connectionStringSetting, string clientName)
        {
            ConfigurationOptions options = await RedisUtilities.ResolveConfigurationOptionsAsync(configuration, connectionStringSetting, clientName);
            return await ConnectionMultiplexer.ConnectAsync(options);
        }
    }
}