using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal static class RedisUtilities
    {
        public static RedisTriggerType ResolveTriggerType(IConfiguration configuration, string toResolve, string error)
        {
            if (Enum.TryParse<RedisTriggerType>(ResolveString(configuration, toResolve, error), out RedisTriggerType result))
            {
                return result;
            }
            throw new InvalidCastException($"Invalid {error} - key exists in config but not a RedisTriggerType");
        }

        public static bool ResolveBool(IConfiguration configuration, string toResolve, string error)
        {
            if (bool.TryParse(ResolveString(configuration, toResolve, error), out bool result))
            {
                return result;
            }
            throw new InvalidCastException($"Invalid {error} - key exists in config but not a bool");
        }

        public static int ResolveInt(IConfiguration configuration, string toResolve, string error)
        {
            if (int.TryParse(ResolveString(configuration, toResolve, error), out int result))
            {
                if (result < 1)
                {
                    throw new ArgumentException($"Invalid {error} - less than 1");
                }
                return result;
            }
            throw new InvalidCastException($"Invalid {error} - key exists in config but not an int");
        }

        public static string ResolveString(IConfiguration configuration, string toResolve, string error)
        {
            if (string.IsNullOrEmpty(toResolve))
            {
                throw new ArgumentNullException($"Empty {error} key");
            }

            // get string from config using input as key
            if (toResolve.StartsWith("%") && toResolve.EndsWith("%"))
            {
                string configKey = toResolve.Substring(1, toResolve.Length - 2);
                string resolvedString = configuration.GetConnectionStringOrSetting(configKey);
                if (string.IsNullOrEmpty(resolvedString))
                {
                    throw new ArgumentException($"Invalid {error} - key does not exist in the config");
                }
                return resolvedString;
            }

            return toResolve;
        }
    }
}
