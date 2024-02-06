using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisPubSubTriggerBinding : ITriggerBinding
    {
        private readonly IConfiguration configuration;
        private readonly string connectionStringSetting;
        private readonly string channel;
        private readonly bool pattern;
        private readonly Type parameterType;
        private readonly ILogger logger;

        public RedisPubSubTriggerBinding(IConfiguration configuration, string connectionStringSetting, string channel, bool pattern, Type parameterType, ILogger logger)
        {
            this.configuration = configuration;
            this.connectionStringSetting = connectionStringSetting;
            this.channel = channel;
            this.pattern = pattern;
            this.parameterType = parameterType;
            this.logger = logger;
        }

        public Type TriggerValueType => typeof(ChannelMessage);
        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();
        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            ChannelMessage message = (ChannelMessage)value;
            IReadOnlyDictionary<string, object> bindingData = CreateBindingData(message);
            return Task.FromResult<ITriggerData>(new TriggerData(new ChannelMessageValueProvider(message, parameterType), bindingData));
        }

        public async Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisPubSubTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            IConnectionMultiplexer multiplexer = await RedisExtensionConfigProvider.GetOrCreateConnectionMultiplexerAsync(configuration, connectionStringSetting, context.Descriptor.ShortName);
            return new RedisPubSubListener(context.Descriptor.LogName, multiplexer, channel, pattern, context.Executor, logger);
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new RedisPubSubTriggerParameterDescriptor
            {
                Channel = channel
            };
        }

        internal static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            return new Dictionary<string, Type>()
            {
                { nameof(ChannelMessage.SubscriptionChannel), typeof(string) },
                { nameof(ChannelMessage.Channel), typeof(string) },
                { nameof(ChannelMessage.Message), typeof(string) },
            };
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(ChannelMessage message)
        {
            return new Dictionary<string, object>()
            {
                { nameof(ChannelMessage.SubscriptionChannel), message.SubscriptionChannel.ToString() },
                { nameof(ChannelMessage.Channel), message.Channel.ToString() },
                { nameof(ChannelMessage.Message) , message.Message.ToString() },
            };
        }
    }
}
