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
    internal class RedisListTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly TimeSpan pollingInterval;
        private readonly int messagesPerWorker;
        private readonly string key;
        private readonly int count;
        private readonly bool listPopFromBeginning;
        private readonly ILogger logger;

        public RedisListTriggerBinding(string connectionString, string key, TimeSpan pollingInterval, int messagesPerWorker, int count, bool listPopFromBeginning, ILogger logger)
        {
            this.connectionString = connectionString;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.count = count;
            this.listPopFromBeginning = listPopFromBeginning;
            this.logger = logger;
        }

        public Type TriggerValueType => typeof(RedisListEntry);

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
                logger?.LogError($"[{nameof(RedisListTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisListListener(context.Descriptor.LogName, connectionString, key, pollingInterval, messagesPerWorker, count, listPopFromBeginning, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new RedisListTriggerParameterDescriptor
            {
                Keys = keys
            };
        }
    }
}
