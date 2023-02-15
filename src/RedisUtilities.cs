using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Host;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal static class RedisUtilities
    {
        public static string ResolveString(INameResolver nameResolver, string setting, string settingName, ILogger logger)
        {
            if (nameResolver.TryResolveWholeString(setting, out string resolvedString))
            {
                logger?.LogDebug($"The {nameof(INameResolver)} provided resolved {settingName} '{settingName}' as '{resolvedString}'.");
                return resolvedString;
            }
            
            if (!string.IsNullOrWhiteSpace(settingName))
            {
                logger?.LogDebug($"Using the provided {settingName} value '{setting}'.");
            }

            logger?.LogError($"{settingName} is null or empty.");
            throw new ArgumentNullException(settingName);
        }

        public static string ResolveConnectionString(IConfiguration configuration, string connectionStringSetting, ILogger logger)
        {
            if (string.IsNullOrWhiteSpace(connectionStringSetting))
            {
                logger?.LogError($"{nameof(connectionStringSetting)} is null or empty.");
                throw new ArgumentNullException(nameof(connectionStringSetting));
            }

            if (configuration is null)
            {
                logger?.LogError($"{nameof(configuration)} is null.");
                throw new ArgumentException(nameof(configuration));
            }

            string connectionString = configuration.GetConnectionStringOrSetting(connectionStringSetting);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger?.LogError($"{nameof(connectionStringSetting)} '{connectionStringSetting}' does not exist as a connection string or value in the configuration.");
                throw new ArgumentNullException(nameof(configuration));
            }

            return connectionString;
        }
    }
}
