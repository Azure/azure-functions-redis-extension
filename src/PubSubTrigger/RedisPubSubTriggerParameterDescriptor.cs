using Microsoft.Azure.WebJobs.Host.Protocols;
using System;
using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis
{
    internal class RedisPubSubTriggerParameterDescriptor : TriggerParameterDescriptor
    {
        internal string channel { get; set; }

        internal RedisPubSubTriggerParameterDescriptor(string channel)
        {
            this.channel = channel;
        }

        public override string GetTriggerReason(IDictionary<string, string> arguments)
        {
            return $"Redis pubsub message from channel '{channel}' at {DateTime.Now}";
        }
    }
}
