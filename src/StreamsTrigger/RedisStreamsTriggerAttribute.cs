using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class RedisStreamsTriggerAttribute : RedisPollingTriggerAttributeBase
    {
        /// <summary>
        /// If true, the function will delete the stream entries after processing.
        /// </summary>
        public string DeleteAfterProcess { get; set; } = "false";
    }
}