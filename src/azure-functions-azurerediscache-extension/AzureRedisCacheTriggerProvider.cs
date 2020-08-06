using Microsoft.Azure.WebJobs.Host.Triggers;
using Microsoft.Extensions.Configuration;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    ///<summary>
    ///Provides trigger binding, variables configured in local.settings.json are being retrieved here.
    ///</summary>
    class AzureRedisCacheTriggerProvider : ITriggerBindingProvider
    {
        private readonly IConfiguration _configuration;
        public AzureRedisCacheTriggerProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            
            ParameterInfo parameter = context.Parameter;
            AzureRedisCacheTriggerAttribute attribute = parameter.GetCustomAttribute<AzureRedisCacheTriggerAttribute>(inherit: false);
            

            if (attribute == null)
            {
                return Task.FromResult<ITriggerBinding>(null);
            }

            string cacheConnectionString = resolveConnectionString(attribute);
            string channelName = resolveChannelName(attribute);
            bool isKeySpaceNotificationsEnabed = attribute.IsKeySpaceNotificationsEnabled;

            return Task.FromResult<ITriggerBinding>(new AzureRedisCacheExtensionTrigger(cacheConnectionString, channelName, isKeySpaceNotificationsEnabed));
        }

        ///<summary>
        ///Resolves connection string from 'CacheConnection' trigger input parameter.
        ///</summary>
        string resolveConnectionString(AzureRedisCacheTriggerAttribute attributeContext) {
            string unresolvedConnectionString = attributeContext.CacheConnection;

            if (!string.IsNullOrEmpty(unresolvedConnectionString))
            {
                if (unresolvedConnectionString.StartsWith("%") && unresolvedConnectionString.EndsWith("%"))
                {
                    string resolvedConnectionStringKey = unresolvedConnectionString.Substring(1, unresolvedConnectionString.Length - 2);
                    return _configuration.GetConnectionStringOrSetting(resolvedConnectionStringKey);
                }
                else {
                    return unresolvedConnectionString;
                }
            }
            else {
                throw new ArgumentNullException("empty connection string key");
            }

        }

        ///<summary>
        ///Resolves channel name from 'ChannelName' trigger input parameter.
        ///</summary>
        string resolveChannelName(AzureRedisCacheTriggerAttribute attributeContext) {
            string unresolvedChannelName = attributeContext.ChannelName;

            if (!string.IsNullOrEmpty(unresolvedChannelName))
            {
                if (unresolvedChannelName.StartsWith("%") && unresolvedChannelName.EndsWith("%"))
                {
                    string resolvedChannelNameKey = unresolvedChannelName.Substring(1, unresolvedChannelName.Length - 2);
                    return _configuration.GetConnectionStringOrSetting(resolvedChannelNameKey);
                }
                else {
                    return unresolvedChannelName;
                }
            }
            else {
                throw new ArgumentNullException("empty channel name key");
            }
        }

    }
}
