using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs.Extensions.Redis.Samples.Models;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Linq;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    internal class StreamSample
    {
        // Redis connection string and stream names stored in local.settings.json


        /// <summary>
        /// </summary>
        /// <param name="entry"> The message which has gone through the stream. Includes message id alongside the key/value pairs </param>
        /// <param name="items"> Container for where the CosmosDB items are stored </param>
        /// <param name="logger"> ILogger used to write key information </param>
        [FunctionName(nameof(WriteThroughForStream))]
        public static void WriteThroughForStream(
                 [CosmosDB(
                ILogger logger)
        {
            // Insert data into CosmosDB synchronously
        }
    }
}
