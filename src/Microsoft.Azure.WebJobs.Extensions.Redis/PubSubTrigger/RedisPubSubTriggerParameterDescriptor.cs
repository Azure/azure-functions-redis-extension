using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal sealed class RedisPubSubTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        internal string Channel { get; set; }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return $"Redis pubsub message detected from channel '{Channel}' at {DateTime.UtcNow:O}.";
        }
    }
}
