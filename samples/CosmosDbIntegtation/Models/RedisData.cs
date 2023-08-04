using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models
{
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
