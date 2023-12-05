In order to get this to run locally, first build and pack the Microsoft.Azure.WebJobs.Extensions.Redis package.
Then, create a Nuget.Config file at the root of this project with the following content.
Finally, build the dotnet-isolated sample csproj.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="Microsoft.Azure.WebJobs.Extensions.Redis" value=".\src\Microsoft.Azure.WebJobs.Extensions.Redis\bin\Debug\" />
  </packageSources>
</configuration>
```