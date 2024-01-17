 using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples.CosmosDB.Models
{
    public record CosmosDBListData(
        string id,
        List<string> value
        );
}