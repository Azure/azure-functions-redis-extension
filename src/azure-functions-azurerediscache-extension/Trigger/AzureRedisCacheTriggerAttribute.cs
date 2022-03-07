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
        /// <summary>
        /// Gets or sets the cache connection string for azure redis cache instance
        /// </summary>
        [AutoResolve]
        public string CacheConnection { get; set; }

        /// <summary>
        /// Gets or sets the channel name to which the user wants to subscribe
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets the keyspace notification enabled setting
        /// </summary>
        public bool IsKeySpaceNotificationsEnabled { get; set; } = false;

    }
}