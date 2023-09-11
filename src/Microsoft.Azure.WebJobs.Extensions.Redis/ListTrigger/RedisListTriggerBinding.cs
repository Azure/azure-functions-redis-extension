using Microsoft.Azure.WebJobs.Host.Bindings;
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

        public Type TriggerValueType => typeof(RedisValue);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            RedisValue redisValue = (RedisValue)value;
            IReadOnlyDictionary<string, object> bindingData = CreateBindingData(redisValue);
            return Task.FromResult<ITriggerData>(new TriggerData(new RedisValueValueProvider(redisValue, parameterType), bindingData));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            if (context is null)
            {
                logger?.LogError($"[{nameof(RedisListTriggerBinding)}] Provided {nameof(ListenerFactoryContext)} is null.");
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult<IListener>(new RedisListListener(context.Descriptor.LogName, connectionString, key, pollingInterval, maxBatchSize, listPopFromBeginning, context.Executor, logger));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new RedisListTriggerParameterDescriptor
            {
                Key = key
            };
        }

        internal static IReadOnlyDictionary<string, Type> CreateBindingDataContract()
        {
            return new Dictionary<string, Type>()
            {
                { "Value", typeof(string) }
            };
        }

        internal static IReadOnlyDictionary<string, object> CreateBindingData(RedisValue value)
        {
            return new Dictionary<string, object>()
            {
                { "Value", value.ToString() }
            };
        }
    }
}
