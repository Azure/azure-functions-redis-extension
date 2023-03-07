using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.WebJobs.Host;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal static class RedisUtilities
    {
        public static readonly char DELIMITER = ' ';

        public static string[] ResolveDelimitedStrings(INameResolver nameResolver, string setting, string settingName)
        {
            string resolvedString = ResolveString(nameResolver, setting, settingName);
            if (string.IsNullOrWhiteSpace(resolvedString)) {
                return new string[] { };
            }
            return resolvedString.Split(DELIMITER);
        }

        public static string ResolveString(INameResolver nameResolver, string setting, string settingName)
        {
            if (nameResolver is null)
            {
                throw new ArgumentNullException(nameof(nameResolver));
            }

            if (string.IsNullOrWhiteSpace(setting))
            {
                return setting;
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