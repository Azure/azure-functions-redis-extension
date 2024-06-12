using System;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models
{
    public record RedisData(
        string id,
        string key,
        string value,
        DateTime timestamp
        );
}
