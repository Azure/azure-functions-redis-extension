using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Extensions for Redis integration.
    /// </summary>
    public static class RedisBuilderExtension
    {
        /// <summary>
        /// Adds the Redis extension to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to add the Redis extension to.</param>
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