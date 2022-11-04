using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Redis;

namespace Microsoft.Extensions.Hosting
{
    public static class RedisExtension
    {
        public static IWebJobsBuilder AddRedis(this IWebJobsBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddExtension<RedisExtensionConfigProvider>();
            return builder;
        }
    }
}