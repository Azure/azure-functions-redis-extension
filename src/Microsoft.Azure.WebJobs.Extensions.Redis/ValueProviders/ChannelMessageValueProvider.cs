using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
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
            //if (Type.Equals(typeof(ChannelMessage)))
            //{
            //    return Task.FromResult<object>(message);
            //}
            if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(ToInvokeString());
            }
            if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(ToInvokeString()));
            }
            if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(ToInvokeString())));
            }

            return Task.FromResult(JsonConvert.DeserializeObject(ToInvokeString(), Type));
        }

        /// <summary>
        /// Serializes <see cref="ChannelMessage"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(new Dictionary<string, string>()
            {
                { nameof(ChannelMessage.SubscriptionChannel), message.SubscriptionChannel.ToString() },
                { nameof(ChannelMessage.Channel), message.Channel.ToString() },
                { nameof(ChannelMessage.Message) , message.Message.ToString() },
            });
        }
    }
}