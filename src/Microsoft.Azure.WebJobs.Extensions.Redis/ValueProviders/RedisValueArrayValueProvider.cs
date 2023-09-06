using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Linq;
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
            else if (Type.Equals(typeof(string[])))
            {
                return Task.FromResult<object>(values.ToStringArray());
            }
            else
            {
                return Task.FromResult<object>(values.Select(value => RedisUtilities.RedisValueTypeConverter(value, Type.GetElementType())).ToArray());
            }
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