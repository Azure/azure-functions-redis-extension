using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Hosting;

[assembly: WebJobsStartup(typeof(RedisExtensionStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the WebJobBuilder on the functions host.
    /// </summary>
    public class RedisExtensionStartup : IWebJobsStartup
    {
        /// <summary>
        /// Adds Redis triggers and bindings to the WebJobBuilder on the functions host.
        /// </summary>
        /// <param name="builder"></param>
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddRedis();
        }
    }
}