﻿using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string key;
        private readonly int maxBatchSize;
        private readonly Type parameterType;
        private readonly ILogger logger;

        public RedisStreamTriggerBinding(string connectionString, string key, TimeSpan pollingInterval, int maxBatchSize, Type parameterType, ILogger logger)
        {
            this.connectionString = connectionString;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.maxBatchSize = maxBatchSize;
            this.parameterType = parameterType;
            this.logger = logger;
        }

        public Type TriggerValueType => parameterType.IsArray ? typeof(StreamEntry[]) : typeof(StreamEntry);
        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            if (parameterType.IsArray && parameterType != typeof(byte[]) && parameterType != typeof(NameValueEntry[]))
            {
                StreamEntry[] entries = (StreamEntry[])value;
                IReadOnlyDictionary<string, object> bindingData = CreateBindingData(entries);
                return Task.FromResult<ITriggerData>(new TriggerData(new StreamEntryArrayValueProvider(entries, parameterType), bindingData));
            }
            else
            {
                StreamEntry entry = (StreamEntry)value;
                IReadOnlyDictionary<string, object> bindingData = CreateBindingData(entry);
                return Task.FromResult<ITriggerData>(new TriggerData(new StreamEntryValueProvider(entry, parameterType), bindingData));
            }
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisStreamTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisStreamListener(
                context.Descriptor.LogName,
                connectionString,
                key,
                pollingInterval,
                maxBatchSize,
                parameterType.IsArray && parameterType != typeof(byte[]) && parameterType != typeof(NameValueEntry[]),
                context.Descriptor.Id,
                context.Executor,
                logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new RedisStreamTriggerParameterDescriptor
            {
                Key = key
            };
        }


        internal IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            if (parameterType.IsArray)
            {
                return new Dictionary<string, Type>()
                {
                    { "Key", typeof(string) },
                    { "Count", typeof(int) }
                };
            }
            else
            {
                return new Dictionary<string, Type>()
                {
                    { "Key", typeof(string) },
                    { nameof(StreamEntry.Id), typeof(string) },
                    { nameof(StreamEntry.Values), typeof(Dictionary<string, string>) },
                };
            }
        }

        internal IReadOnlyDictionary<string, object> CreateBindingData(StreamEntry entry)
        {
            return new Dictionary<string, object>()
            {
                { "Key", key },
                { nameof(StreamEntry.Id), entry.Id.ToString() },
                { nameof(StreamEntry.Values), RedisUtilities.StreamEntryToDictionary(entry) },
            };
        }

        internal IReadOnlyDictionary<string, object> CreateBindingData(StreamEntry[] entries)
        {
            return new Dictionary<string, object>()
            {
                { "Key", key },
                { "Count", entries.Length },
            };
        }
    }
}