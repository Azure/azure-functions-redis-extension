using Microsoft.Azure.WebJobs.Host.Bindings;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="ChannelMessage"/>.
    /// </summary>
    public class ChannelMessageValueProvider : IValueProvider
    {
        private readonly ChannelMessage message;

        /// <summary>
        /// Value provider for <see cref="ChannelMessage"/>.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="destinationType"></param>
        public ChannelMessageValueProvider(ChannelMessage message, Type destinationType)
        {
            this.message = message;
            Type = destinationType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="ChannelMessage"/> into the requested parameter type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(ChannelMessage)))
            {
                return Task.FromResult<object>(message);
            }
            return Task.FromResult(RedisUtilities.RedisValueTypeConverter(message.Message, Type));
        }

        /// <summary>
        /// Serializes <see cref="ChannelMessage"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return message.Message.ToString();
        }
    }
}