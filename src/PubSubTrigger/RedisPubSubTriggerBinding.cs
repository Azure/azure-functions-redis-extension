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
        private readonly ILogger logger;

        public RedisPubSubTriggerBinding(string connectionString, string channel, ILogger logger)
        {
            this.connectionString = connectionString;
            this.channel = channel;
            this.logger = logger;
        }

        public Type TriggerValueType => typeof(RedisPubSubMessage);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>();
            return Task.FromResult<ITriggerData>(new TriggerData(null, bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisPubSubTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisPubSubListener(connectionString, channel, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}
