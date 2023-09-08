using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="RedisValue"/>[].
    /// </summary>
    public class RedisValueArrayValueProvider : IValueProvider
    {
        private readonly RedisValue[] values;

        /// <summary>
        /// Value provider for <see cref="RedisValue"/>[].
        /// </summary>
        /// <param name="values"></param>
        /// <param name="destinationType"></param>
        public RedisValueArrayValueProvider(RedisValue[] values, Type destinationType)
        {
            this.values = values;
            Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="RedisValue"/>[] into the requested parameter type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(RedisValue[])))
            {
                return Task.FromResult<object>(values);
            }
            if (Type.Equals(typeof(string[])))
            {
                return Task.FromResult<object>(values.ToStringArray());
            }
            if (Type.Equals(typeof(byte[][])))
            {
                return Task.FromResult<object>(Array.ConvertAll(values, value => (byte[])value));
            }
            if (Type.Equals(typeof(ReadOnlyMemory<byte>[])))
            {
                return Task.FromResult<object>(Array.ConvertAll(values, value => (ReadOnlyMemory<byte>)value));
            }

            string msg = $@"Binding parameters to complex objects (such as '{Type.GetElementType().Name}') is not supported for batches.";
            throw new InvalidOperationException(msg);
        }

        /// <summary>
        /// Serializes <see cref="RedisValue"/>[] into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(values.ToStringArray());
        }
    }
}
