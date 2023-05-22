## Setup Function Project
1. Follow the [Configure your local environment](https://learn.microsoft.com/en-us/azure/azure-functions/create-first-function-cli-node?pivots=nodejs-model-v4&tabs=azure-cli%2Cbrowser#configure-your-local-environment) instructions for Javascript.
1. Install the [.NET SDK](https://aka.ms/dotnet-download)
1. Create a node function project with the following commands:
   ```
    mkdir RedisFunctions
    cd RedisFunctions
    func init --worker-runtime node
    ```
1. Install the Redis Extension
   ```
   func extensions install --package Microsoft.Azure.WebJobs.Extensions.Redis --version <version>
   ```
   `<version>` should be the latest version of the extension from [NuGet](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Redis)
1. Create a folder `PubSubTrigger`, and add the following two files into the folder:

   `function.json`:
   ```json
   {
     "bindings": [
       {
         "type": "redisPubSubTrigger",
         "connectionStringSetting": "redisLocalhost",
         "channel": "pubsubTest",
         "name": "message",
         "direction": "in"
       }
     ],
     "scriptFile": "index.js"
   }
   ```

   `index.js`:
   ```js
   module.exports = async function (context, entry) {
      context.log('Javascript function triggered on pubsub message \'' + message + '\' from channel \'pubsubTest\'.');
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
        "FUNCTIONS_WORKER_RUNTIME": "python",
        "Redis": "<connectionString>"
      }
    }
    ```

## Run Function
1. Start the function locally:
   ```
   func start
   ```
1. Connect to your redis cache using [redis-cli](https://redis.io/docs/ui/cli/), [RedisInsight](https://redis.com/redis-enterprise/redis-insight/) or some other redis client.
1. Publish a message to the channel `pubsubTest`:
   ```
   publish pubsubTest testing
   ```
1. Your function should trigger!
