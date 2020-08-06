using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.AzureRedisCache;
using System;

namespace Microsoft.Extensions.Hosting
{
    ///<summary>
    ///Build IWebJob from new AddAzureRedisCache instance.
    ///</summary>
    public static class AzureRedisCacheExtension
    {
        public static IWebJobsBuilder AddAzureRedisCache(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<AzureRedisCacheExtensionConfigProvider>();
            return builder;
        }
    }
}