namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// The different types of notifications that the function will trigger on.
    /// </summary>
    public enum RedisTriggerType
    {
        PubSub,
        ShardedPubSub,
        KeySpace,
        KeyEvent,
        Streams,
        List
    }
}
