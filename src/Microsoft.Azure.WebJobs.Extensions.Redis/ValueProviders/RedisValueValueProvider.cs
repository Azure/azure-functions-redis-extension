using Microsoft.Azure.WebJobs.Host.Bindings;
using StackExchange.Redis;
using System;
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
            Type = destinationType;
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
            return Task.FromResult(RedisUtilities.RedisValueTypeConverter(value, Type));
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