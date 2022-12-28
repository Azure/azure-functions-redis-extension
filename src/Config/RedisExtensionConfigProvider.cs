using System;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

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
            // trigger
            FluentBindingRule<RedisTriggerAttribute> triggerRule = context.AddBindingRule<RedisTriggerAttribute>();
            triggerRule.BindToTrigger<RedisMessageModel>(new RedisTriggerBindingProvider(configuration));

            // connection multiplexer binding
            FluentBindingRule<RedisConnectionAttribute> connectionRule = context.AddBindingRule<RedisConnectionAttribute>();
            connectionRule.BindToInput(new RedisConnectionConverter(configuration));

            // command binding
            FluentBindingRule<RedisCommandAttribute> commandRule = context.AddBindingRule<RedisCommandAttribute>();
            commandRule.BindToInput(new RedisCommandConverter(configuration));

            // script binding
            FluentBindingRule<RedisScriptAttribute> scriptRule = context.AddBindingRule<RedisScriptAttribute>();
            scriptRule.BindToInput(new RedisScriptConverter(configuration));
#pragma warning restore CS0618
<<<<<<< HEAD
=======
            rule.BindToTrigger<RedisMessageModel>(new RedisTriggerBindingProvider(configuration));


#pragma warning disable CS0618
            FluentBindingRule<RedisStreamsTriggerAttribute> streamsTriggerRule = context.AddBindingRule<RedisStreamsTriggerAttribute>();
#pragma warning restore CS0618
            streamsTriggerRule.BindToTrigger<RedisStream[]>(new RedisStreamsTriggerBindingProvider(configuration));
>>>>>>> 770767b (move all existing trigger classes under PubSubTrigger folder, and create completely new StreamsTrigger)
        }
    }
}
