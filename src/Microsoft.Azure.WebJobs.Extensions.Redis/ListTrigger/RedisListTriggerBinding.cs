﻿using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
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
        private readonly string key;
        private readonly int maxBatchSize;
        private readonly bool listPopFromBeginning;
        private readonly Type parameterType;
        private readonly ILogger logger;

        public RedisListTriggerBinding(string connectionString, string key, TimeSpan pollingInterval, int maxBatchSize, bool listPopFromBeginning, Type parameterType, ILogger logger)
        {
            this.connectionString = connectionString;
            this.key = key;
            this.pollingInterval = pollingInterval;
            this.maxBatchSize = maxBatchSize;
            this.listPopFromBeginning = listPopFromBeginning;
            this.parameterType = parameterType;
            this.logger = logger;
        }

        public Type TriggerValueType => parameterType.IsArray ? typeof(RedisValue[]) : typeof(RedisValue);

        public IReadOnlyDictionary<string, Type> BindingDataContract => CreateBindingDataContract();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            if (parameterType.IsArray && parameterType != typeof(byte[]))
            {
                RedisValue[] redisValues = (RedisValue[])value;
                IReadOnlyDictionary<string, object> bindingData = CreateBindingData(redisValues);
                return Task.FromResult<ITriggerData>(new TriggerData(new RedisValueArrayValueProvider(redisValues, parameterType), bindingData));
            }
            else
            {
                RedisValue redisValue = (RedisValue)value;
                IReadOnlyDictionary<string, object> bindingData = CreateBindingData(redisValue);
                return Task.FromResult<ITriggerData>(new TriggerData(new RedisValueValueProvider(redisValue, parameterType), bindingData));
            }
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisListTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisListListener(
                context.Descriptor.LogName,
                connectionString,
                key,
                pollingInterval,
                maxBatchSize,
                listPopFromBeginning,
                parameterType.IsArray && parameterType != typeof(byte[]),
                context.Executor,
                logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new RedisListTriggerParameterDescriptor
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
                    { "Value", typeof(string) }
                };
            }
        }

        internal IReadOnlyDictionary<string, object> CreateBindingData(RedisValue value)
        {
            return new Dictionary<string, object>()
            {
                { "Key", key },
                { "Value", value.ToString() }
            };
        }

        internal IReadOnlyDictionary<string, object> CreateBindingData(RedisValue[] values)
        {
            return new Dictionary<string, object>()
            {
                { "Key", key },
                { "Count", values.Length }
            };
        }
    }
}
