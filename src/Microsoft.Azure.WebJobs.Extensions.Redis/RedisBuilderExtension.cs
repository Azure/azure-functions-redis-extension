using Microsoft.Azure.WebJobs.Host.Scale;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
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
            builder.Services.AddAzureClientsCore();
            return builder;
        }

        /// <summary>
        /// Adds the <see cref="RedisScalerProvider"/> to the provided <see cref="IWebJobsBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IWebJobsBuilder"/> to add the <see cref="RedisScalerProvider"/> to.</param>
        /// <param name="triggerMetadata">The metadata for the trigger.</param>
        /// <returns></returns>
        internal static IWebJobsBuilder AddRedisScaleForTrigger(this IWebJobsBuilder builder, TriggerMetadata triggerMetadata)
        {
            IServiceProvider serviceProvider = null;
            Lazy<RedisScalerProvider> scalerProvider = new Lazy<RedisScalerProvider>(() => new RedisScalerProvider(serviceProvider, triggerMetadata));

            builder.Services.AddSingleton<IScaleMonitorProvider>(resolvedServiceProvider =>
            {
                serviceProvider = serviceProvider ?? resolvedServiceProvider;
                return scalerProvider.Value;
            });

            builder.Services.AddSingleton<ITargetScalerProvider>(resolvedServiceProvider =>
            {
                serviceProvider = serviceProvider ?? resolvedServiceProvider;
                return scalerProvider.Value;
            });

            return builder;
        }
    }
}