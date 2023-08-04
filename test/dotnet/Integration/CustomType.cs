using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class CustomType
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string Random { get; set; }
    }

    public record RedisData(
        string id,
        string key,
        string value,
        DateTime timestamp
    );

    public record PubSubData(
        string id,
        string channel,
        string message,
        DateTime timestamp
        );
}
