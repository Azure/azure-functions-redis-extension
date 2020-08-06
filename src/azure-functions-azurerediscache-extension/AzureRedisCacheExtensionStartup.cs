using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: WebJobsStartup(typeof(AzureRedisCacheExtensionStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{
    ///<summary>
    ///AzureRedisCache extension startup.
    ///</summary>
    public class AzureRedisCacheExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddAzureRedisCache();
        }
    }
}