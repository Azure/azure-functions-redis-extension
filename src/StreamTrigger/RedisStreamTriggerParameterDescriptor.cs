using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisStreamTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        internal string keys { get; set; }

        internal RedisStreamTriggerParameterDescriptor(string keys)
        {
            this.keys = keys;
        }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return $"Redis stream entry from key '{keys}' at {DateTime.Now}";
        }
    }
}
