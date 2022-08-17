using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    ///<summary>
    ///Trigger Binding, manages and binds context to listener.
    ///</summary>
    public class AzureRedisCacheExtensionTrigger : ITriggerBinding
    {
        private string _cacheConnectionString;
        private string _channelName;
        private bool _isKeySpaceNotificationsEnabled;

        public AzureRedisCacheExtensionTrigger(string connectionString, string channelName, bool isKeySpaceNotificationsEnabled) 
        {
            _cacheConnectionString = connectionString;
            _channelName = channelName;
            _isKeySpaceNotificationsEnabled = isKeySpaceNotificationsEnabled;
        }

        public Type TriggerValueType => typeof(AzureRedisCacheMessageModel);

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

            return Task.FromResult<IListener>(new AzureRedisCacheListener(_cacheConnectionString, _channelName, _isKeySpaceNotificationsEnabled, context.Executor));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor();
        }
    }
}
