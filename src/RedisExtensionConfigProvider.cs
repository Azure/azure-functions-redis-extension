using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    [Extension("Redis")]
    public class RedisExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration configuration;
        public RedisExtensionConfigProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Initializes binding to trigger provider via binding rule.
        /// </summary>
        public void Initialize(ExtensionConfigContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

#pragma warning disable CS0618
            // connection multiplexer binding
            FluentBindingRule<RedisConnectionAttribute> connectionRule = context.AddBindingRule<RedisConnectionAttribute>();
            connectionRule.BindToInput(new RedisConnectionConverter(configuration));

            // command binding
            FluentBindingRule<RedisCommandAttribute> commandRule = context.AddBindingRule<RedisCommandAttribute>();
            commandRule.BindToInput(new RedisCommandConverter(configuration));

            // script binding
            FluentBindingRule<RedisScriptAttribute> scriptRule = context.AddBindingRule<RedisScriptAttribute>();
            scriptRule.BindToInput(new RedisScriptConverter(configuration));

            FluentBindingRule<RedisPubSubTriggerAttribute> rule = context.AddBindingRule<RedisPubSubTriggerAttribute>();
            rule.BindToTrigger<RedisMessageModel>(new RedisPubSubTriggerBindingProvider(configuration));

            FluentBindingRule<RedisStreamsTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamsTriggerAttribute>();
            streamsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisStreamsTriggerBindingProvider(configuration));

            FluentBindingRule<RedisListsTriggerAttribute> listsTriggerRule = context.AddBindingRule<RedisListsTriggerAttribute>();
            listsTriggerRule.BindToTrigger<RedisMessageModel>(new RedisListsTriggerBindingProvider(configuration));
#pragma warning restore CS0618
        }
    }
}
