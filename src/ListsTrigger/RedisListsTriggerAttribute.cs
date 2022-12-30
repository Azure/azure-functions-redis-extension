using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Streams trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class RedisListsTriggerAttribute : RedisPollingTriggerAttributeBase
    {
        /// <summary>
        /// Decides if the function will pop elements from the front or end of the list.
        /// True (default) = pop elements from the front of the list.
        /// False = pop elements from the end of the list.
        /// </summary>
        public bool ListPopFromBeginning { get; set; } = true;
    }
}