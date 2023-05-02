using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal sealed class RedisStreamTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        internal string Keys { get; set; }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return $"Redis stream entry detected from key '{Keys}' at {DateTime.UtcNow:O}.";
        }
    }
}
