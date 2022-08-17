using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;


namespace AzureRedisCacheDemoFunction
{
    public static class AzureRedisCacheDemoFunction
    {
        
        [FunctionName("AzureRedisCacheDemoFunctionPubSub")]
        public static void Run(
            [AzureRedisCacheTrigger(CacheConnection = "%CacheConnection%", ChannelName = "%ChannelName%")]
            AzureRedisCacheMessageModel result, ILogger logger)
        {
            logger.LogInformation($"Pub-Sub Message: {result.Message}");
            //logger.LogInformation($"key {result.Key} had an event: {result.KeySpaceNotification}");
        }
        
    }
}
