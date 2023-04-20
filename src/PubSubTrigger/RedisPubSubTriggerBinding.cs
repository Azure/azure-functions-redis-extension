using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisPubSubTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly string channel;
        private readonly Type parameterType;
        private readonly ILogger logger;

        public RedisPubSubTriggerBinding(string connectionString, string channel, Type parameterType, ILogger logger)
        {
            this.connectionString = connectionString;
            this.channel = channel;
            this.parameterType = parameterType;
            this.logger = logger;
        }

        public Type TriggerValueType => typeof(RedisPubSubMessage);
        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();
        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            RedisPubSubMessage message = (RedisPubSubMessage)value;
            IReadOnlyDictionary<string, object> bindingData = CreateBindingData(message);
            return Task.FromResult<ITriggerData>(new TriggerData(new RedisPubSubMessageValueProvider(message, parameterType), bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisPubSubTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisPubSubListener(context.Descriptor.LogName, connectionString, channel, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }

        internal static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            return new Dictionary<string, Type>()
            {
                { nameof(RedisPubSubMessage.SubscriptionChannel), typeof(string) },
                { nameof(RedisPubSubMessage.Channel), typeof(string) },
                { nameof(RedisPubSubMessage.Message), typeof(string) },
            };
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(RedisPubSubMessage message)
        {
            return new Dictionary<string, object>()
            {
                { nameof(message.SubscriptionChannel), message.SubscriptionChannel },
                { nameof(message.Channel), message.Channel },
                { nameof(message.Message) , message.Message },
            };
        }
    }
}
