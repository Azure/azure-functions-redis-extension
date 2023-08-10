# Tutorial: Setup Redis Triggers for Azure Function in Visual Studio Code

## Summary
The following tutorial shows how to implement database caching samples using Azure Cache for Redis and Azure CosmosDB. You will incorporate basic triggers using Azure Functions. The goal is to synchorize data between Azure Cache for Redis and Azure CosmosDB. You will install necessary packages and incorporate samples using different caching patterns.

In this tutorial, you learn how to:
1. [Install packages in Azure Function and adding sample code according to each Redis trigger and caching pattern](#install-packages-in-azure-function-and-adding-sample-code-according-to-each-redis-trigger-and-caching-pattern)
2. [Adding connection string and variables to local.settings.json and ListSamples](#adding-connection-string-and-variables-to-local.settings.json-and-listsamples)


## Prerequisites
- Accounts
   - Azure Cache for Redis
   - Azure CosmosDB Account
- Visual Studio Code
- Completion of a previous Tutorial
    - [How to configure Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-configure)
        - Required in order to use each Azure Function trigger
    - [Create an Azure Cosmos DB account, database, container, and items from the Azure portal](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/quickstart-portal)
        - Finish steps up to (and including), "Add a database and a container"
        - Required in order to store data in Azure CosmosDB for each Azure Function trigger
    - [Get started with Azure Functions triggers in Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-tutorial-functions-getting-started)
        - Finish steps up to (and including), "Configure the cache"
        - Required in set up your Azure Cache for Redis with packages and to create an Azure Function in Visual Studio Code

## Install packages in Azure Function and adding sample code according to each Redis trigger and caching pattern
1. Install the packages for Azure CosmosDB:\
    `dotnet add package Microsoft.Azure.Cosmos`\
    `dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB`\
    `dotnet add package Microsoft.Azure.WebJobs.Extensions.Redis --prerelease`\
    `dotnet add package Microsoft.NET.Sdk.Functions`
1. Create a new C# file in your Azure Function project to implement the sample code.
1. Create another C# file in your Azure Function project to incorporate custom data models from the [RedisData.cs](Models/RedisData.cs) file.
2. Choose a sample below to use with your Azure Function according to each Azure Function trigger and caching pattern.
    - ReadThrough
        - [PubSub.Sample](ReadThroughSamples/PubSubSample.cs)
        - [List.Sample](ReadThroughSamples/ListSample.cs)
    - WriteAround
        - [PubSub.Sample](WriteAroundSamples/PubSubSample.cs)
        - [List.Sample](WriteAroundSamples/ListSample.cs)
        - [Stream.Sample](WriteAroundSamples/StreamSample.cs)
    - WriteBehind
        - [PubSub.Sample](WriteBehindSamples/PubSubSample.cs)
        - [List.Sample](WriteBehindSamples/ListSample.cs)
        - [Stream.Sample](WriteBehindSamples/StreamSample.cs)
    - WriteThrough
        - [PubSub.Sample](WriteThroughSamples/PubSubSample.cs)
        - [Stream.Sample](WriteThroughSamples/StreamSample.cs)

3. Copy and paste the sample code into your Azure Function, using the first C# file you created.

## Adding connection string and variables to local.settings.json and ListSamples
1. Navigate to your local.settings.json file in Visual Studio.
2. Copy and paste the following code as a replacement for your local.settings.json.
3. Replace each variable with the appropriate value.
 ```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "CosmosDbDatabaseId": "<database-id>",
    "ListCosmosDbContainerId": "<container-id-for-list>",
    "PubSubCosmosDbContainerId": "<container-id-for-pubsub>",
    "MessagesCosmosDbContainerId": "<container-id-for-pubsub-messages>",
    "PubSubChannel": "<pubsub-channel-name>",
    "StreamCosmosDbContainerId": "<container-id-for-stream>",
    "StreamCosmosDbContainerIdSingleDocument": "<container-id-for-stream-singular-document>",
    "StreamTest": "<stream-name>",
    "StreamTestSingleDocument": "<stream-name-singular-document>",
    "RedisConnectionString": "<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=",
    "CosmosDbConnectionString": "AccountEndpoint=https://<cosmosdb-account>.documents.azure.com:443/;AccountKey=<access-key>;"
  }
}
 ```
> **Note:**
> Each trigger should be using a separate container, which is dependent on the variable names.\
> **Note:**
> The containers used must be created in the Azure CosmosDB account before running the Azure Function.\
> Only containers for the triggers you are using need to be created.

 ### Description of each variable
| Variable name                           | Description                                                                                               |
|-----------------------------------------|-----------------------------------------------------------------------------------------------------------|
| CosmosDbDatabaseId                      | Database all of the functions can share                                                                   |
| ListCosmosDbContainerId                 | Container for Lists                                                                                       |
| PubSubCosmosDbContainerId               | Container for key/value pairs set with PubSub triggers                                                    |
| MessagesCosmosDbContainerId             | Container for messages sent with PubSub triggers                                                          |
| PubSubChannel                           | The name of the channel the PubSub trigger is listening to                                                |
| StreamCosmosDbContainerId               | Container IDs of the CosmosDB containers that store the stream messages                                   |
| StreamCosmosDbContainerIdSingleDocument | Container IDs of the CosmosDB containers that store the stream messages when writing to a single document |
| StreamTest                              | Name of the streams the trigger is listening to                                                           |
| StreamTestSingleDocument                | Name of the stream the trigger is listening when writing to a single document in CosmosDB                 |
| RedisConnectionString                   | Primary connection string to Redis Cache                                                                  |
| CosmosDbConnectionString                | Account endpoint to CosmosDB                                                                              |


### Adjusting List variables in ListSamples
Lists 
1.	WriteBehind
    - Change the value of ListKey to the desired key before the function 
2.	WriteAround 
    - Change the value of ListKey to the desired key before the function 
3. ReadThrough
    - Change the value of ListKey to the desired key before the function
    ```csharp
    public const string ListKey = "userListName";
    ```


### Security
* This design introduces new secrets. Theses secrets are the CosmosDB keys and connection strings, and the Azure Cache for Redis Connection strings. These will be stored in the customers’ storage account associated with their function app.

## Clean up resources

If you're not going to continue to use this application, delete
<resources> with the following steps:

1. From the homepage of the Azure Portal, navigate to the resource you want to delete
2. On overview, select Delete account or “Delete."
3. Confirm anything else to delete the resource


## Related content

> [README](README.md)


