﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisStreamsTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly TimeSpan pollingInterval;
        private readonly int messagesPerWorker;
        private readonly string keys;
        private readonly int count;
        private readonly string consumerGroup;
        private readonly bool deleteAfterProcess;
        private readonly ILogger logger;

        public RedisStreamsTriggerBinding(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int count, string consumerGroup, bool deleteAfterProcess, ILogger logger)
        {
            this.connectionString = connectionString;
            this.keys = keys;
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.count = count;
            this.consumerGroup = consumerGroup;
            this.deleteAfterProcess = deleteAfterProcess;
            this.logger = logger;
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
            if (context is null)
            {
                logger?.LogCritical($"Provided {nameof(TriggerBindingProviderContext)} is null.");
                throw new ArgumentNullException("context");
            }

            return Task.FromResult<IListener>(new RedisStreamsListener(connectionString, keys, pollingInterval, messagesPerWorker, count, consumerGroup, deleteAfterProcess, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}