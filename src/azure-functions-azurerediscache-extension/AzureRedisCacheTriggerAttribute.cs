using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]

    ///<summary>
    ///Trigger input binding attributes
    ///</summary>
    public class AzureRedisCacheTriggerAttribute : Attribute
    {
        [AutoResolve]
        public string CacheConnection { get; set; }
        public string ChannelName { get; set; }
        public bool IsKeySpaceNotificationsEnabled { get; set; } = false;

    }
}