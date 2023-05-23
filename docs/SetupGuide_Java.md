## Setup Function Project
1. Follow the [Configure your local environment](https://learn.microsoft.com/azure/azure-functions/create-first-function-vs-code-java#configure-your-environment) instructions for Java.
1. Install the [.NET SDK](https://aka.ms/dotnet-download)
1. Create a Java function project with `mvn archetype:generate -DarchetypeGroupId=com.microsoft.azure -DarchetypeArtifactId=azure-functions-archetype -DjavaVersion=8`
1. Remove the `test` folder for now.
1. Install the Redis Extension (manually for now, while the extension has not been added to the Microsoft.Azure.Functions.ExtensionBundle)
   1. Remove `extensionBundle` from `host.json`
   1. Run `func extensions install --package Microsoft.Azure.WebJobs.Extensions.Redis --version <version>`
      - `<version>` should be the latest version of the extension from [NuGet](https://www.nuget.org/packages/Microsoft.Azure.WebJobs.Extensions.Redis)
   1. Add the Java library for Redis bindings to the `pom.xml` file:
      ```xml
      <dependency>
        <groupId>com.microsoft.azure.functions</groupId>
        <artifactId>azure-functions-java-library-redis</artifactId>
        <version>[0.0.0,)</version>
      </dependency>
      ```
1. Replace the existing `Function.java` file with the following code:
    ```java
    import com.microsoft.azure.functions.*;
    import com.microsoft.azure.functions.annotation.*;
    import com.microsoft.azure.functions.redis.annotation.*;

    public class Function {
      @FunctionName("PubSubTrigger")
      public void PubSubTrigger(
        @RedisPubSubTrigger(
          name = "message",
          connectionStringSetting = "Redis",
          channel = "pubsubTest")
          String message,
        final ExecutionContext context) {
        context.getLogger().info("Java function triggered on pub/sub message '" + message + "' from channel 'pubsubTest'.");
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
        "AzureWebJobsStorage": "",
        "FUNCTIONS_WORKER_RUNTIME": "java",
        "Redis": "<connectionString>"
      }
    }
    ```

## Run Function locally
1. Start the function locally:
   ```
   mvn clean package
   mvn azure-functions:run
   ```
1. Connect to your Redis cache using [redis-cli](https://redis.io/docs/ui/cli/), [RedisInsight](https://redis.com/redis-enterprise/redis-insight/) or some other Redis client.
1. Publish a message to the channel `pubsubTest`:
   ```
   PUBLISH pubsubTest testing
   ```
1. Your function should trigger!