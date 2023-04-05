using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis Stream entry.
    /// </summary>
    public class RedisStreamEntry
    {
        /// <summary>
        /// Redis Stream entry.
        /// </summary>
        /// <param name="key">The stream key that the function was triggered on.</param>
        /// <param name="id">The ID assigned to the entry.</param>
        /// <param name="values">The values contained within the entry.</param>
        public RedisStreamEntry(string key, string id, KeyValuePair<string, string>[] values)
        {
            Key = key;
            Id = id;
            Values = values;
        }

        /// <summary>
        /// The stream key that the function was triggered on.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The ID assigned to the entry.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The values contained within the entry.
        /// </summary>
        public KeyValuePair<string, string>[] Values { get; }
    }
}
