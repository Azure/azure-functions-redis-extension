using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal static class RedisUtilities
    {
        public static Version Version62 = new Version("6.2");
        public static Version Version70 = new Version("7.0");

        public static string ResolveString(INameResolver nameResolver, string setting, string settingName)
        {
            if (nameResolver is null)
            {
                throw new ArgumentNullException(nameof(nameResolver));
            }

            if (string.IsNullOrWhiteSpace(setting))
            {
                throw new ArgumentNullException(settingName);
            }

            if (nameResolver.TryResolveWholeString(setting, out string resolvedString))
            {
                return resolvedString;
            }

            return setting;
        }

        public static string ResolveConnectionString(IConfiguration configuration, string connectionStringSetting)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(connectionStringSetting))
            {
                throw new ArgumentNullException(nameof(connectionStringSetting));
            }

            string connectionString = configuration.GetConnectionStringOrSetting(connectionStringSetting);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentOutOfRangeException($"Could not find {nameof(connectionStringSetting)}='{connectionStringSetting}' in provided configuration.");
            }

            return connectionString;
        }
    }
}