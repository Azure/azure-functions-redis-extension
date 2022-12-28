using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using StackExchange.Redis;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisStreamsTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly string[] keys;
        private readonly string consumerGroup;
        private readonly string consumerName;
        private readonly int count;
        private readonly int pollingInterval;


        public RedisStreamsTriggerBinding(string connectionString, string keys, string consumerGroup, string consumerName, int count, int pollingInterval)
        {
            this.connectionString = connectionString;
            this.keys = keys.Split(' ');
            this.consumerGroup = consumerGroup;
            this.consumerName = consumerName;
            this.count = count;
            this.pollingInterval = pollingInterval;
        }

        public Type TriggerValueType => typeof(RedisStream[]);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            IReadOnlyDictionary<string, object> bindingData = new Dictionary<string, object>();
            return Task.FromResult<ITriggerData>(new TriggerData(null, bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            return Task.FromResult<IListener>(new RedisStreamsListener(connectionString, keys, consumerGroup, consumerName, count, pollingInterval, context.Executor));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}
