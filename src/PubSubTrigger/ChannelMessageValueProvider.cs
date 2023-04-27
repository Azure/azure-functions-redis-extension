using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
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
            this.Type = destinationType;
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
            if (Type.Equals(typeof(RedisValue)))
            {
                return Task.FromResult<object>(message.Message);
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message.Message.ToString())));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(message.Message.ToString()));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(message.Message.ToString());
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
        /// Serializes <see cref="ChannelMessage"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            return JsonConvert.SerializeObject(message, ChannelMessageConverter.Instance);
        }

        internal sealed class ChannelMessageConverter : JsonConverter<ChannelMessage>
        {
            internal static readonly ChannelMessageConverter Instance = new ChannelMessageConverter();
            public override ChannelMessage ReadJson(JsonReader reader, Type objectType, ChannelMessage existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, ChannelMessage value, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName(nameof(ChannelMessage.SubscriptionChannel));
                writer.WriteValue(value.SubscriptionChannel.ToString());
                writer.WritePropertyName(nameof(ChannelMessage.Channel));
                writer.WriteValue(value.Channel.ToString());
                writer.WritePropertyName(nameof(ChannelMessage.Message));
                writer.WriteValue(value.Message.ToString());
                writer.WriteEndObject();
            }
        }
    }
}