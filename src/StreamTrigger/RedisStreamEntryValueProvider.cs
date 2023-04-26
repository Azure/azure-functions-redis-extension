using Microsoft.Azure.WebJobs.Host.Bindings;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="RedisStreamEntry"/>.
    /// </summary>
    public class RedisStreamEntryValueProvider : IValueProvider
    {
        private readonly RedisStreamEntry entry;

        /// <summary>
        /// Value provider for <see cref="RedisStreamEntry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="destinationType"></param>
        public RedisStreamEntryValueProvider(RedisStreamEntry entry, Type destinationType)
        {
            this.entry = entry;
            this.Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="RedisStreamEntry"/> into the requested type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(RedisStreamEntry)))
            {
                return Task.FromResult<object>(entry);
            }
            else if (Type.Equals(typeof(KeyValuePair<string, string>[])))
            {
                return Task.FromResult<object>(entry.Values);
            }
            else if (Type.Equals(typeof(IReadOnlyDictionary<string, string>)))
            {
                return Task.FromResult<object>(entry.Values.ToDictionary());
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry.Values))));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(entry.Values)));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(JsonConvert.SerializeObject(entry.Values));
            }

            else
            {
                try
                {
                    return Task.FromResult(JsonConvert.DeserializeObject(ToInvokeString(), Type));
                }
                catch (JsonException e)
                {
                    // Give useful error if object in queue is not deserialized properly.
                    string msg = $@"Binding parameters to complex objects (such as '{Type.Name}') uses Json.NET serialization. The JSON parser failed: {e.Message}";
                    throw new InvalidOperationException(msg, e);
                }
            }
        }

        /// <summary>
        /// Serializes <see cref="RedisStreamEntry"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(entry);
        }
    }
}