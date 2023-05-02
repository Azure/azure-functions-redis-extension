using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class CustomChannelMessage
    {
        public string SubscriptionChannel { get; }
        public string Channel { get; }
        public string Message { get; }

        public CustomChannelMessage(string subscriptionChannel, string channel, string message)
        {
            SubscriptionChannel = subscriptionChannel;
            Channel = channel;
            Message = message;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
