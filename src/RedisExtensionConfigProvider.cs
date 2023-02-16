using System;
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
        private readonly INameResolver nameResolver;
        /// <summary>
        /// Adds Redis triggers and bindings to the extension context.
        /// </summary>
        /// <param name="configuration"></param>
        public RedisExtensionConfigProvider(IConfiguration configuration, INameResolver nameResolver)
        {
            this.configuration = configuration;
            this.nameResolver = nameResolver;
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
            FluentBindingRule<RedisCommandAttribute> commandRule = context.AddBindingRule<RedisCommandAttribute>();
            commandRule.BindToInput(new RedisCommandConverter(configuration));

            FluentBindingRule<RedisScriptAttribute> scriptRule = context.AddBindingRule<RedisScriptAttribute>();
            scriptRule.BindToInput(new RedisScriptConverter(configuration));

            FluentBindingRule<RedisPubSubTriggerAttribute> pubsubTriggerRule = context.AddBindingRule<RedisPubSubTriggerAttribute>();
            pubsubTriggerRule.BindToTrigger<RedisMessageModel>(new RedisPubSubTriggerBindingProvider(configuration, nameResolver));

            FluentBindingRule<RedisListsTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListsTriggerAttribute>();
            listsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisListsTriggerBindingProvider(configuration, nameResolver));

            FluentBindingRule<RedisStreamsTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamsTriggerAttribute>();
            streamsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisStreamsTriggerBindingProvider(configuration, nameResolver));
#pragma warning restore CS0618
        }
    }
}
