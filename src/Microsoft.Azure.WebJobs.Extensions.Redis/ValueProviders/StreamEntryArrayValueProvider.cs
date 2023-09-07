using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="StreamEntry"/>[].
    /// </summary>
    public class StreamEntryArrayValueProvider : IValueProvider
    {
        private readonly StreamEntry[] entries;

        /// <summary>
        /// Value provider for <see cref="StreamEntry"/>[].
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="parameterType"></param>
        public StreamEntryArrayValueProvider(StreamEntry[] entries, Type parameterType)
        {
            this.entries = entries;
            Type = parameterType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="StreamEntry"/>[] into the requested parameter type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(StreamEntry[])))
            {
                return Task.FromResult<object>(entries);
            }
            if (Type.Equals(typeof(NameValueEntry[][])))
            {
                return Task.FromResult<object>(entries.Select(e => e.Values).ToArray());
            }
            if (Type.Equals(typeof(Dictionary<string, string>[])))
            {
                return Task.FromResult<object>(entries.Select(e => RedisUtilities.StreamEntryToDictionary(e)).ToArray());
            }
            if (Type.Equals(typeof(ReadOnlyMemory<byte>[])))
            {
                return Task.FromResult<object>(entries.Select(e => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToString(e)))).ToArray());
            }
            if (Type.Equals(typeof(byte[][])))
            {
                return Task.FromResult<object>(entries.Select(e => Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToString(e))).ToArray());
            }
            if (Type.Equals(typeof(string[])))
            {
                return Task.FromResult<object>(entries.Select(e => RedisUtilities.StreamEntryToString(e)).ToArray());
            }
            try
            {
                return Task.FromResult<object>(entries.Select(e => JsonConvert.DeserializeObject(JsonConvert.SerializeObject(RedisUtilities.StreamEntryToDictionary(e)), Type.GetElementType())).ToArray());
            }
            catch (JsonException e)
            {
                string msg = $@"Binding parameters to complex objects (such as '{Type.GetElementType().Name}') uses Json.NET serialization. The JSON parser failed: {e.Message}";
                throw new InvalidOperationException(msg, e);
            }
        }

        /// <summary>
        /// Serializes <see cref="StreamEntry"/>[] into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(entries.Select(e => RedisUtilities.StreamEntryToString(e)));
        }
    }
}
