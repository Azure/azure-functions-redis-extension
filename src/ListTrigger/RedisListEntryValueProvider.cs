using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="RedisListEntry"/>.
    /// </summary>
    public class RedisListEntryValueProvider : IValueProvider
    {
        private readonly RedisListEntry entry;

        /// <summary>
        /// Value provider for <see cref="RedisListEntry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="destinationType"></param>
        public RedisListEntryValueProvider(RedisListEntry entry, Type destinationType)
        {
            this.entry = entry;
            this.Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="RedisListEntry"/> into the requested object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(RedisListEntry)))
            {
                return Task.FromResult<object>(entry);
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(entry.Value)));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(entry.Value));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(entry.Value);
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
        /// Serializes <see cref="RedisListEntry"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(entry);
        }
    }
}