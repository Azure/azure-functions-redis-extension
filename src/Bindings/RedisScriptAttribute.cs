using System;
using Microsoft.Azure.WebJobs.Description;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// An input binding that excutes a command on the redis instances and returns the reult.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Binding]
    public sealed class RedisScriptAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionStringSetting"></param>
        /// <param name="luaScript"></param>
        /// <param name="keys"></param>
        /// <param name="args"></param>
        public RedisScriptAttribute(string connectionStringSetting, string luaScript, string keys = "", string args = "") {
            ConnectionStringSetting = connectionStringSetting;
            LuaScript = luaScript;
            Keys = keys;
            Args = args;
        }

        /// <summary>
        /// Redis connection string setting.
        /// This setting will be used to resolve the actual connection string from the appsettings.
        /// </summary>
        [AutoResolve]
        public string ConnectionStringSetting { get; }

        /// <summary>
        /// The lua script to be executed on the cache.
        /// </summary>
        [AutoResolve]
        public string LuaScript { get; }

        /// <summary>
        /// Space-delimited keys for the script.
        /// </summary>
        [AutoResolve]
        public string Keys { get; }

        /// <summary>
        /// Space-delimited values for the script.
        /// </summary>
        [AutoResolve]
        public string Args { get; }
    }
}
