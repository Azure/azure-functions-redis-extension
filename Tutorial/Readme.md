# Get Started Using Azure Cache for Redis as a Trigger for Azure Functions

## Summary
Azure Cache for Redis can now be used as a trigger to execute Azure Functions. Three Redis data types can be used to trigger Azure Function execution:

- [Redis Pub/Sub messages](https://redis.io/docs/manual/pubsub/)
- [Redis lists](https://redis.io/docs/data-types/lists/)
- [Redis streams](https://redis.io/docs/data-types/streams/)

The Pub/Sub trigger also enables Redis [keyspace notifications](https://redis.io/docs/manual/keyspace-notifications/) to be used as triggers. Keyspace notifications enable Redis to fire events to a pub/sub channel when keys are modified, or operations are performed. These events can then be used as triggers for Azure Functions, executing code once the event has occurred. This enables a variety of powerful use-cases, such as utilizing Azure Cache for Redis in a write-behind configuration or as a part of an event-driven architecture. The following actions are supported through keyspace notification as triggers:

- All supported commands that affect a given key (e.g. the key is updated, or set to expire)
- All uses of a specific supported command. (e.g. DEL, SET, or RENAME)

The list of [commands supported by keyspace notifications is listed here](https://redis.io/docs/manual/keyspace-notifications/). 

All tiers of Azure Cache for Redis are supported with the Redis trigger. 

## Unsupported Functionality
This feature is still in private preview, so there are a few limitations that will be removed in the future:
- Only .NET functions are supported. Java, Node, and Python functions will also be supported after the feature enters public preview.
- Only durable functions (i.e. Premium functions) are supported. We’ll support non-durable (i.e. Consumption functions) for public preview in the Basic, Standard, and Premium tiers.
- Azure Cache for Redis functions bindings are not yet supported.
- The Pub/Sub trigger is not capable of listening to keyspace notifications on clustered caches.

## Other Important Information
More information on the Redis Extension can be found [here](https://github.com/Azure/azure-functions-redis-extension. Documentation and source code are available. 
This feature is still in a private preview state. The product team is eager to get your feedback! Please email redisfunctionpreview@microsoft.com with any questions, suggestions, bugs, or feedback. 

## Getting Started Tutorial

The following tutorial shows how to implement basic triggers with Azure Cache for Redis and Azure Functions. Note that some of the steps will change as the product develops. This tutorial uses VS Code to write and deploy the Azure Function. It’s also possible to do the same thing using Visual Studio. In the future, you will be able to do this in the Azure portal as well.

### Requirements

- Azure subscription
- Visual Studio Code
- Custom NuGet package (available in this repo)

### 1. Set up an Azure Cache for Redis Instance

Create a new **Azure Cache for Redis** instance using the Azure portal or your preferred CLI tool. Any tier and SKU should work. We’ll use a _Standard C1_ instance, which is a good starting tier and SKU. 

The default settings should suffice. We’ll use a public endpoint for this demo, but you’ll likely want to use a private endpoint for anything in production. 
The cache can take a bit to create, so feel free to move to the next section while this completes. 

### 2. Set up Visual Studio Code

If you haven’t installed the functions extension for VS Code, do so by searching for _Azure Functions_ in the extensions menu and selecting Install. If you don’t have the C# extension installed, please install that as well. 
 
Next, go to the **Azure** tab, and sign-in to your existing Azure account, or create a new one:
 
Create a new local folder on your computer to hold the project that we’ll be building. I’ve named mine “AzureRedisFunctionDemo”
In the Azure tab, create a new functions app by clicking on the lightning icon in the top right of the **Workspace** box in the lower left of the screen.

Select the new folder that you’ve created. This will start the creation of a new Azure Functions project. You’ll get several on-screen prompts. Select:

- **C#** as the language
- **.NET 6.0 LTS** as the .NET runtime
- **Skip for now** as the project template
Note: If you don’t have the .NET Core SDK installed, you’ll be prompted to do so.

The new project will be created:

### 3. Install Necessary NuGet packages

You’ll need to install two NuGet packages:
1. [StackExchange.Redis](https://www.nuget.org/packages/StackExchange.Redis/), which is the primary .NET client for Redis. 
1. Microsoft.Azure.WebJobs.Extensions.Redis, which is the extension that allows Redis keyspace notifications to be used as triggers in Azure Functions. 

Install StackExchange.Redis by going to the **Terminal** tab in VS Code and entering the following command:

`dotnet add package StackExchange.Redis`

Next, we need to install the Microsoft.Azure.WebJobs.Extensions.Redis package. When this feature is released publically, this will be very simple. But we have to jump through some additional hoops right now. Follow the instructions listed in step #4 here. 

### 4. Configure Cache

Go to your newly created Azure Cache for Redis instance. Two steps need to be taken here. 
First, we need to enable **keyspace notifications** on the cache to trigger on keys and commands. Go to your cache in the Azure portal and select the **Advanced settings** blade. Scroll down to the field labled _notify-keyspace-events_ and enter “KEA”. Then select Save at the top of the window. “KEA” is a configuration string that enables keyspace notifications for all keys and events. More information on keyspace configuration strings can be found [here](https://redis.io/docs/manual/keyspace-notifications/). 

Second, go to the Access keys blade and copy the Primary connection string field. We’ll use this to connect to the cache. 
 


