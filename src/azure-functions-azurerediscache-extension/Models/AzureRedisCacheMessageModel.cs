namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    ///<summary>
    ///This model gets returned when the function is triggered.
    ///</summary>
    public class AzureRedisCacheMessageModel
    {
        public string Channel { get; set; }
        public string Message { get; set; }
        public string Key { get; set; }
        public string KeySpaceNotification { get; set; }
    }
}
