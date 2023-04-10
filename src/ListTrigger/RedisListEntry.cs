namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Redis List entry.
    /// </summary>
    public class RedisListEntry
    {
        /// <summary>
        /// Redis List entry.
        /// </summary>
        /// <param name="key">The list key that the function was triggered on.</param>
        /// <param name="value">The value that was pushed to the list.</param>
        public RedisListEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// The list key that the function was triggered on.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// The value that was pushed to the list.
        /// </summary>
        public string Value { get; }
    }
}
