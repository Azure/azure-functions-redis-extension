using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="RedisValue"/>.
    /// </summary>
    public class RedisValueValueProvider : IValueProvider
    {
        private readonly RedisValue value;

        /// <summary>
        /// Value provider for <see cref="RedisValue"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        public RedisValueValueProvider(RedisValue value, Type destinationType)
        {
            this.value = value;
            this.Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="RedisValue"/> into the requested parameter type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(RedisValue)))
            {
                return Task.FromResult<object>(value);
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(value.ToString())));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(value.ToString()));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(value.ToString());
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
        /// Serializes <see cref="RedisValue"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return value.ToString();
        }
    }
}