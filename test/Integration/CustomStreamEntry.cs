namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class CustomStreamEntry
    {
        public string SubscriptionChannel { get; set; }
        public string Channel { get; set; }
        public string Message { get; set; }
    }
}
