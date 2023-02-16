﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using StackExchange.Redis;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Trigger Binding, manages and binds context to listener.
    /// </summary>
    internal class RedisListsTriggerBinding : ITriggerBinding
    {
        private readonly string connectionString;
        private readonly string keys;
        private readonly TimeSpan pollingInterval;
        private readonly int messagesPerWorker;
        private readonly int count;
        private readonly bool listPopFromBeginning;
        private readonly ILogger logger;

        public RedisListsTriggerBinding(string connectionString, string keys, TimeSpan pollingInterval, int messagesPerWorker, int count, bool listPopFromBeginning, ILogger logger)
        {
            this.connectionString = connectionString;
            this.keys = keys;
            this.pollingInterval = pollingInterval;
            this.messagesPerWorker = messagesPerWorker;
            this.count = count;
            this.listPopFromBeginning = listPopFromBeginning;
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
                logger?.LogCritical($"Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(ListenerFactoryContext));
            }

            return Task.FromResult<IListener>(new RedisListsListener(connectionString, keys, pollingInterval, messagesPerWorker, count, listPopFromBeginning, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}
