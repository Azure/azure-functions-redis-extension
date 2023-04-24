using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for RedisPubSubMessage.
    /// </summary>
    public class RedisPubSubMessageValueProvider : IValueProvider
    {
        private readonly RedisPubSubMessage message;

        /// <summary>
        /// Value provider for RedisPubSubMessage.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destinationType"></param>
        public RedisPubSubMessageValueProvider(RedisPubSubMessage message, Type destinationType)
        {
            this.message = message;
            this.Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the RedisPubSubMessage into the requested object.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(RedisPubSubMessage)))
            {
                return Task.FromResult<object>(message);
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message.Message)));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(message.Message));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(message.Message);
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
        /// Serializes RedisStreamEntry into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(message);
        }
    }
}