using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Azure;
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

        public const string EntraFullyQualifiedCacheName = "fullyQualifiedCacheName";
        public const string EntraPrincipalId = "principalId";
        public const string EntraClientId = "clientId";

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

        public static async Task<ConfigurationOptions> ResolveConfigurationOptionsAsync(IConfiguration configuration, AzureComponentFactory azureComponentFactory, string connectionStringSetting, string clientName)
        {
            ConfigurationOptions options;

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(connectionStringSetting))
            {
                throw new ArgumentNullException(nameof(connectionStringSetting));
            }

            IConfigurationSection section = configuration.GetWebJobsConnectionSection(connectionStringSetting);
            string connectionString = section.Value;
            string cacheHostName = section[EntraFullyQualifiedCacheName];
            if (string.IsNullOrWhiteSpace(connectionString) && string.IsNullOrWhiteSpace(cacheHostName))
            {
                throw new ArgumentException($"{nameof(connectionStringSetting)} '{connectionStringSetting}' not found in provided configuration.");

            }
            if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(cacheHostName))
            {
                throw new ArgumentException($"Found both {nameof(connectionStringSetting)} '{connectionStringSetting}' and '{connectionStringSetting}__{EntraFullyQualifiedCacheName}' in provided configuration. Please choose either connection string or managed identity connection.");
            }

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options = ConfigurationOptions.Parse(connectionString);
            }
            else
            {
                // Entra Id Connections
                string principalId = section[EntraPrincipalId];

                if (string.IsNullOrWhiteSpace(principalId))
                {
                    throw new ArgumentNullException($"{connectionStringSetting}__{EntraPrincipalId}");
                }

                options = await ConfigurationOptions.Parse(cacheHostName).ConfigureForAzureWithTokenCredentialAsync(principalId, azureComponentFactory.CreateTokenCredential(section));
            }

            options.ClientName = GetRedisClientName(clientName);
            return options;
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

        public static string GetRedisClientName(string clientName) => $"AzureFunctionsRedisExtension.{clientName}";
    }
}
