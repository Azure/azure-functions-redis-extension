using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: WebJobsStartup(typeof(RedisExtensionStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    public class RedisExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddRedis();
        }
    }
}