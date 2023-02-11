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
        private readonly int pollingInterval;
        private readonly int messagesPerWorker;
        private readonly string keys;
        private readonly int count;
        private readonly string consumerGroup;
        private readonly bool deleteAfterProcess;

        public RedisStreamsTriggerBinding(string connectionString, int pollingInterval, int messagesPerWorker, string keys, int count, string consumerGroup, bool deleteAfterProcess)
        {
            this.connectionString = connectionString;
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.keys = keys;
            this.count = count;
            this.consumerGroup = consumerGroup;
            this.deleteAfterProcess = deleteAfterProcess;
        }

        public Type TriggerValueType => typeof(RedisMessageModel);

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

            return Task.FromResult<IListener>(new RedisStreamsListener(connectionString, pollingInterval, messagesPerWorker, keys, count, consumerGroup, deleteAfterProcess, context.Executor));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}
