﻿using Microsoft.Azure.WebJobs.Description;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis lists trigger binding attributes.
    /// </summary>
    [Binding]
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RedisListTriggerAttribute : RedisPollingTriggerBaseAttribute
    {
        /// <summary>
        /// Initializes a new <see cref="RedisListTriggerAttribute"/>.
        /// </summary>
        /// <param name="connection">App setting name that contains Redis connection information.</param>
        /// <param name="key">Key to read from.</param>
        /// <param name="pollingIntervalInMs">How often to poll Redis in milliseconds. Default: 1000</param>
        /// <param name="maxBatchSize">Number of entries to pull from a Redis list at one time. Default: 16</param>
        /// <param name="listDirection">The direction to pop elements from the list. Default: left</param>
        public RedisListTriggerAttribute(string connection, string key, int pollingIntervalInMs = 1000, int maxBatchSize = 16, ListDirection listDirection = ListDirection.LEFT)
            : base(connection, key, pollingIntervalInMs, maxBatchSize)
        {
            ListDirection = listDirection;
        }

        /// <summary>
        /// The direction to pop elements from the list.
        /// </summary>
        public ListDirection ListDirection { get; }
    }

    /// <summary>
    /// The direction to pop elements from the list.
    /// </summary>
    public enum ListDirection
    {
        /// <summary>
        /// Uses <a href="https://redis.io/commands/lpop/">LPOP</a>
        /// </summary>
        LEFT,

        /// <summary>
        /// Uses <a href="https://redis.io/commands/rpop/">RPOP</a>
        /// </summary>
        RIGHT
    }
}