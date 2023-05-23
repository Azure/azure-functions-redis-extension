## Setup Function Project
1. Install the [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local).
1. Install the [.NET SDK](https://aka.ms/dotnet-download)
1. Create a .NET out-of-process function project with the following commands:
   ```
    mkdir RedisFunctions
    cd RedisFunctions
    func init --worker-runtime dotnet-isolated
    ```
1. Install the Redis Extension
   ```
   dotnet add package Microsoft.Azure.Functions.Worker.Extensions.Redis --prerelease
   ```
1. Create a `Function.cs` file with the following code:
    ```c#
    using Microsoft.Extensions.Logging;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Extensions.Redis;

    public class Function
    {
        private readonly ILogger<Function> logger;

        public Function(ILogger<Function> logger)
        {
            this.logger = logger;
        }

        [Function("PubSubTrigger")]
        public void Run(
        [RedisPubSubTrigger("Redis", "pubsubTest")] string message)
        {
        logger.LogInformation($".NET out-of-process function triggered on pub/sub message '{message}' from channel 'pubsubTest'.");
        }
    }
    ```

## Setup Redis Cache
1. [Set up an Azure Cache for Redis instance](https://learn.microsoft.com/azure/azure-cache-for-redis/quickstart-create-redis) or [install Redis locally](https://redis.io/download/).
1. Add the connection string from your Redis instance to your `local.settings.json` file.
   It should look something like this:
    ```json
    {
      "IsEncrypted": false,
      "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "Redis": "<connectionString>"
      }
    }
    ```

## Run Function
1. Start the function locally:
   ```
   func start
   ```
1. Connect to your Redis cache using [redis-cli](https://redis.io/docs/ui/cli/), [RedisInsight](https://redis.com/redis-enterprise/redis-insight/) or some other Redis client.
1. Publish a message to the channel `pubsubTest`:
   ```
   PUBLISH pubsubTest testing
   ```
1. Your function should trigger!

