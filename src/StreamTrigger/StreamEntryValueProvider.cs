﻿using Microsoft.Azure.WebJobs.Host.Bindings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    /// <summary>
    /// Value provider for <see cref="StreamEntry"/>.
    /// </summary>
    public class StreamEntryValueProvider : IValueProvider
    {
        private readonly StreamEntry entry;

        /// <summary>
        /// Value provider for <see cref="StreamEntry"/>.
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="parameterType"></param>
        public StreamEntryValueProvider(StreamEntry entry, Type parameterType)
        {
            this.entry = entry;
            this.Type = parameterType;
        }

        /// <summary>
        /// Requested parameter type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Converts the <see cref="StreamEntry"/> into the requested parameter type.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Task<object> GetValueAsync()
        {
            if (Type.Equals(typeof(StreamEntry)))
            {
                return Task.FromResult<object>(entry);
            }
            else if (Type.Equals(typeof(NameValueEntry[])))
            {
                return Task.FromResult<object>(entry.Values);
            }
            else if (Type.Equals(typeof(Dictionary<string, string>)))
            {
                return Task.FromResult<object>(entry.Values.ToDictionary(value => value.Name.ToString(), value => value.Value.ToString()));
            }
            else if (Type.Equals(typeof(ReadOnlyMemory<byte>)))
            {
                return Task.FromResult<object>(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToValuesJArray(entry).ToString())));
            }
            else if (Type.Equals(typeof(byte[])))
            {
                return Task.FromResult<object>(Encoding.UTF8.GetBytes(RedisUtilities.StreamEntryToValuesJArray(entry).ToString()));
            }
            else if (Type.Equals(typeof(string)))
            {
                return Task.FromResult<object>(RedisUtilities.StreamEntryToValuesJArray(entry).ToString());
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
        /// Serializes <see cref="StreamEntry"/> into a string.
        /// </summary>
        /// <returns></returns>
        public string ToInvokeString()
        {
            JObject obj = new JObject()
            {
                [nameof(StreamEntry.Id)] = entry.Id.ToString(),
                [nameof(StreamEntry.Values)] = RedisUtilities.StreamEntryToValuesJArray(entry)
            };
            return obj.ToString(Formatting.None);
        }
    }
}
