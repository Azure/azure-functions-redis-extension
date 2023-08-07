ms.topic: tutorial #Required; leave this attribute/value as-is
ms.date: #Required; mm/dd/yyyy format


# Tutorial: Setup Redis Triggers for Azure Function in Visual Studio

In this tutorial, you learn how to:
1. Install packages on an existing Azure function
1. Use a Redis trigger with your Azure Function
1. Deploy an Azure Function


## Prerequisites
- Accounts
    - Azure Redis Cache 
   - CosmosDB Account
- Completion of a previous Tutorial
    - [How to configure Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-configure)
    - [Create an Azure Cosmos DB account, database, container, and items from the Azure portal](https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/quickstart-portal)
        - Finish steps up to (and including) "Add a database and a container."
    - [Get started with Azure Functions triggers in Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-tutorial-functions-getting-started)
        - Finish steps up to (and including), "Configure the cache."

- Sample code
    - ReadThrough
        - [PubSub.Sample](samples/CosmosDbIntegtation/ReadThroughSamples/PubSubSample.cs)
        - [List.Sample](samples/CosmosDbIntegtation/ReadThroughSamples/ListSample.cs)
    - WriteAround
        - [PubSub.Sample](samples/CosmosDbIntegtation/WriteAroundSamples/PubSubSample.cs)
        - [List.Sample](samples/CosmosDbIntegtation/WriteAroundSamples/ListSample.cs)
        - [Stream.Sample](samples/CosmosDbIntegtation/WriteAroundSamples/StreamSample.cs)
    - WriteBehind
        - [PubSub.Sample](samples/CosmosDbIntegtation/WriteBehindSamples/PubSubSample.cs)
        - [List.Sample](samples/CosmosDbIntegtation/WriteBehindSamples/ListSample.cs)
        - [Stream.Sample](samples/CosmosDbIntegtation/WriteBehindSamples/StreamSample.cs)
    - WriteThrough
        - [PubSub.Sample](samples/CosmosDbIntegtation/WriteThroughSamples/PubSubSample.cs)
        - [Stream.Sample](samples/CosmosDbIntegtation/WriteThroughSamples/StreamSample.cs)

<!-- 6. Account sign in --------------------------------------------------------------------

Required: If you need to sign in to the portal to do the Tutorial, this H2 and link are required.

-->

<!--## Sign in to 
Home - Microsoft Azure
TODO: add your instructions-->

## Installing Packages and adjusting variables

### Install packages and adding code
1. Add packages
    1. dotned add package Microsoft.Azure.Cosmos
    1. dotnet add package Microsoft.Azure.WebJobs.Extensions.CosmosDB
    1. dotnet add package Microsoft.Azure.WebJobs.Extensions.Redis --prerelease
    1. dotnet add package Microsoft.NET.Sdk.Functions
1. Choose a sample from the Prerequisites above to use with your Azure Function according to each trigger and caching pattern

### Adding connection string and variables
1. Direct to your local.settings.json file in Visual Studio
2. Copy and paste the following code as a replacement for your local.settings.json

 ```json
 {
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ListContainerId": "CosmosDbContainerId",
    "CosmosDbDatabaseId": "DatabaseId",
    "CosmosDbContainerId": "ContainerId",
    "PubSubContainerId": "PSContainerId",
    "PubSubChannel": "PubSubTest",
    "RedisConnectionString": "<cache-name>.redis.cache.windows.net:6380,password=<access-key>,ssl=True,abortConnect=False,tiebreaker=",
    "CosmosDbConnectionString": "AccountEndpoint=https://<cosmosdb-account>.documents.azure.com:443/;AccountKey=<access-key>;"
 }
 ```
2.	Replace the value of the redisConnectionString with your primary connection string. 
3.	Replace the value of CosmosDbDatabaseId with your database name. 
4.	Replace the value of CosmosDbContainerId with your container name 
5.	Replace the value of CosmosDBConnectionString with your CosmosDB Account Endpoint 

### Adjusting List and Streams variables
Lists 
1.	WriteBehind
    - Change the value of key to the desired key before the function 
2.	WriteAround 
    - Change the value of key to the desired key before the function 
 
Streams 
1.	Set streamName variable equal to the name of your stream to connect stream triggers to it. 


## Clean up resources

If you're not going to continue to use this application, delete
<resources> with the following steps:

1. From the Homepage of the Azure Portal, navigate to the resource you want to delete
1. On overview, select Delete account or “Delete
1. Confirm anything else to delete the resource

<!-- 9. Next step/Related content ------------------------------------------------------------------------ 

Optional: You have two options for manually curated links in this pattern: Next step and Related content. You don't have to use either, but don't use both.
  - For Next step, provide one link to the next step in a sequence. Use the blue box format
  - For Related content provide 1-3 links. Include some context so the customer can determine why they would click the link. Add a context sentence for the following links.

-->

## Next step

TODO: Add your next step link(s)

> [!div class="nextstepaction"]
> [Write concepts](article-concept.md)

<!-- OR -->

## Related content

TODO: Add your next step link(s)

- [Write concepts](article-concept.md)
