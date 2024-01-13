using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal static class RedisUtilities
    {
        public const string RedisStreamTrigger = "RedisStreamTrigger";
        public const string RedisListTrigger = "RedisListTrigger";
        public const string RedisPubSubTrigger = "RedisPubSubTrigger";
        public const string RedisInputBinding = "RedisInputBinding";
        public const string RedisOutputBinding = "RedisOutputBinding";
        public const string RedisClientNameFormat = "AzureFunctionsRedisExtension.{0}";

        public const string EntraFullyQualifiedCacheHostName = "{0}__fullyQualifiedCacheHostName";
        public const string EntraPrincipalId = "{0}__principalId";
        public const string EntraTenantId = "{0}__tenantId";
        public const string EntraClientId = "{0}__clientId";
        public const string EntraClientSecret = "{0}__clientSecret";

        public const char BindingDelimiter = ' ';
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

        public static async Task<ConfigurationOptions> ResolveConfigurationOptionsAsync(IConfiguration configuration, string connectionStringSetting)
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
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                return ConfigurationOptions.Parse(connectionString);
            }

            string cacheHostName = configuration.GetConnectionStringOrSetting(string.Format(EntraFullyQualifiedCacheHostName, connectionStringSetting));
            if (!string.IsNullOrWhiteSpace(cacheHostName))
            {
                // Entra Id Connections
                string principalId = configuration.GetConnectionStringOrSetting(string.Format(EntraPrincipalId, connectionStringSetting));
                string clientId = configuration.GetConnectionStringOrSetting(string.Format(EntraClientId, connectionStringSetting));
                string tenantId = configuration.GetConnectionStringOrSetting(string.Format(EntraTenantId, connectionStringSetting));
                string clientSecret = configuration.GetConnectionStringOrSetting(string.Format(EntraClientSecret, connectionStringSetting));

                if (string.IsNullOrWhiteSpace(principalId))
                {
                    throw new ArgumentOutOfRangeException($"Could not find {string.Format(EntraPrincipalId, connectionStringSetting)} in provided configuration.");
                }

                ConfigurationOptions configurationOptions = ConfigurationOptions.Parse(cacheHostName);
                if (string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(tenantId) && string.IsNullOrWhiteSpace(clientSecret))
                {
                    // System-Assigned Managed Identity
                    return await configurationOptions.ConfigureForAzureWithSystemAssignedManagedIdentityAsync(principalId); 
                }

                if (!string.IsNullOrWhiteSpace(clientId) && string.IsNullOrWhiteSpace(tenantId) && string.IsNullOrWhiteSpace(clientSecret))
                {
                    // User-Assigned Managed Identity
                    return await configurationOptions.ConfigureForAzureWithUserAssignedManagedIdentityAsync(clientId, principalId);
                }

                if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(tenantId) && !string.IsNullOrWhiteSpace(clientSecret))
                {
                    // Service Principal
                    return await configurationOptions.ConfigureForAzureWithServicePrincipalAsync(clientId, principalId, tenantId, clientSecret);
                }

                throw new ArgumentOutOfRangeException($"Managed Identity configuration error. {nameof(EntraFullyQualifiedCacheHostName)}={cacheHostName}, {nameof(EntraPrincipalId)}={principalId}, {nameof(EntraClientId)}={clientId}, {nameof(EntraTenantId)}={tenantId}, {nameof(EntraClientSecret)}={clientSecret}.");
            }

            throw new ArgumentOutOfRangeException($"Could not find {nameof(connectionStringSetting)}='{connectionStringSetting}' or '{string.Format(EntraFullyQualifiedCacheHostName, connectionStringSetting)}' in provided configuration.");
        }

        public static object RedisValueTypeConverter(RedisValue value, Type destinationType)
        {
            if (destinationType.Equals(typeof(RedisValue)))
            {
                return value;
            }
            if (destinationType.Equals(typeof(string)))
            {
                return (string)value;
            }
            if (destinationType.Equals(typeof(byte[])))
            {
                return (byte[])value;
            }
            if (destinationType.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return (ReadOnlyMemory<byte>)value;
            }

            try
            {
                return JsonConvert.DeserializeObject((string)value, destinationType);
            }
            catch (JsonException e)
            {
                string msg = $@"Binding parameters to complex objects (such as '{destinationType.Name}') uses Json.NET serialization. The JSON parser failed: {e.Message}";
                throw new InvalidOperationException(msg, e);
            }
        }

        public static Dictionary<string, string> StreamEntryToDictionary(StreamEntry entry)
        {
            return entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString());
        }

        public static string StreamEntryToString(StreamEntry entry)
        {
            JObject obj = new JObject()
            {
                [nameof(StreamEntry.Id)] = entry.Id.ToString(),
                [nameof(StreamEntry.Values)] = JObject.FromObject(RedisUtilities.StreamEntryToDictionary(entry))
            };
            return obj.ToString(Formatting.None);
        }
    }
}
