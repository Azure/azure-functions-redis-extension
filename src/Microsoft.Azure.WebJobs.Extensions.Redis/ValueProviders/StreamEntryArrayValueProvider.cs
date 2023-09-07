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
                return Task.FromResult<object>(Array.ConvertAll(entries, e => e.Values));
            }
            if (Type.Equals(typeof(Dictionary<string, string>[])))
            {
                return Task.FromResult<object>(Array.ConvertAll(entries, e => RedisUtilities.StreamEntryToDictionary(e)));
            }
            if (Type.Equals(typeof(ReadOnlyMemory<byte>[])))
            {
                return Task.FromResult<object>(Array.ConvertAll(entries, e => new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToString(e)))));
            }
            if (Type.Equals(typeof(byte[][])))
            {
                return Task.FromResult<object>(Array.ConvertAll(entries, e => Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToString(e))));
            }
            if (Type.Equals(typeof(string[])))
            {
                return Task.FromResult<object>(Array.ConvertAll(entries, e => RedisUtilities.StreamEntryToString(e)));
            }

            string msg = $@"Binding parameters to complex objects (such as '{Type.GetElementType().Name}') is not supported for batches.";
            throw new InvalidOperationException(msg);
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
