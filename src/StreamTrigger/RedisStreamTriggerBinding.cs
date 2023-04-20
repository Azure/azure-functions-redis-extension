using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisStreamTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly TimeSpan pollingInterval;
        private readonly int messagesPerWorker;
        private readonly string keys;
        private readonly int count;
        private readonly string consumerGroup;
        private readonly bool deleteAfterProcess;
        private readonly Type parameterType;
        private readonly ILogger logger;

        public RedisStreamTriggerBinding(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int count, string consumerGroup, bool deleteAfterProcess, Type parameterType, ILogger logger)
        {
            this.connectionString = connectionString;
            this.keys = keys;
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.count = count;
            this.consumerGroup = consumerGroup;
            this.deleteAfterProcess = deleteAfterProcess;
            this.parameterType = parameterType;
            this.logger = logger;
        }

        public Type TriggerValueType => typeof(RedisStreamEntry);
        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            RedisStreamEntry entry = (RedisStreamEntry)value;
            IReadOnlyDictionary<string, object> bindingData = CreateBindingData(entry);
            return Task.FromResult<ITriggerData>(new TriggerData(new RedisStreamEntryValueProvider(entry, parameterType), bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisStreamTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisStreamListener(context.Descriptor.LogName, connectionString, keys, pollingInterval, messagesPerWorker, count, consumerGroup, deleteAfterProcess, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }

        internal static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            return new Dictionary<string, Type>()
            {
                { nameof(RedisStreamEntry.Key), typeof(string) },
                { nameof(RedisStreamEntry.Id), typeof(string) },
                { nameof(RedisStreamEntry.Values), typeof(KeyValuePair<string, string>[]) }
            };
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(RedisStreamEntry entry)
        {
            return new Dictionary<string, object>()
            {
                { nameof(entry.Key), entry.Key },
                { nameof(entry.Id), entry.Id },
                { nameof(entry.Values), entry.Values },
            };
        }
    }
}