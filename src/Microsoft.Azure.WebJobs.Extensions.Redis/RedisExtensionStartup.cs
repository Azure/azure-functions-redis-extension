using Microsoft.Azure.WebJobs.Extensions.Redis;
using Microsoft.Azure.WebJobs.Hosting;

[assembly: WebJobsStartup(typeof(RedisWebJobsStartup))]
namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Adds Redis triggers and bindings to the WebJobBuilder on the functions host.
    /// </summary>
    public class RedisWebJobsStartup : IWebJobsStartup
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