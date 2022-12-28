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

            // trigger
#pragma warning disable CS0618
            FluentBindingRule<RedisTriggerAttribute> triggerRule = context.AddBindingRule<RedisTriggerAttribute>();
#pragma warning restore CS0618
            triggerRule.BindToTrigger<RedisMessageModel>(new RedisTriggerBindingProvider(configuration));

            // input connection multiplexer binding
#pragma warning disable CS0618
            FluentBindingRule<RedisConnectionAttribute> connectionRule = context.AddBindingRule<RedisConnectionAttribute>();
#pragma warning restore CS0618
            connectionRule.BindToInput(new RedisConnectionConverter(configuration));

            // input connection multiplexer binding
#pragma warning disable CS0618
            FluentBindingRule<RedisCommandAttribute> commandRule = context.AddBindingRule<RedisCommandAttribute>();
#pragma warning restore CS0618
            commandRule.BindToInput(new RedisCommandConverter(configuration));
        }
    }
}
