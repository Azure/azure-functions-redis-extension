using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models
{
    public record PubSubData(
        string id,
        string channel,
        string message,
        DateTime timestamp
        );
}
