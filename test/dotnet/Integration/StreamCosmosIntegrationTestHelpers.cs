using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


// Draft
namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    internal class StreamCosmosIntegrationTestHelpers
    {
        internal static Process StartFunction(string functionName, int port)
        {
            Process functionsProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetFunctionsFileName(),
                    Arguments = $"start --verbose --functions {functionName} --port {port} --no-build --prefix {GetPrefix()}",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            TaskCompletionSource<bool> hostStarted = new TaskCompletionSource<bool>();
            void hostStartupHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data?.Contains($"Host started") ?? false)
                {
                    hostStarted.SetResult(true);
                }
            }
            functionsProcess.OutputDataReceived += hostStartupHandler;

            TaskCompletionSource<bool> functionLoaded = new TaskCompletionSource<bool>();
            void functionLoadedHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data?.Contains($"Generating 1 job function(s)") ?? false)
                {
                    functionLoaded.SetResult(true);
                }
            }
            functionsProcess.OutputDataReceived += functionLoadedHandler;

            functionsProcess.Start();
            functionsProcess.BeginOutputReadLine();
            functionsProcess.BeginErrorReadLine();
            if (!hostStarted.Task.Wait(TimeSpan.FromMinutes(1)))
            {
                throw new Exception("Azure Functions Host did not start");
            }
            if (!functionLoaded.Task.Wait(TimeSpan.FromMinutes(1)))
            {
                throw new Exception($"Did not load Function {functionName}");
            }
            functionsProcess.OutputDataReceived -= hostStartupHandler;
            functionsProcess.OutputDataReceived -= functionLoadedHandler;

            return functionsProcess;
        }

        internal static DataReceivedEventHandler CounterHandlerCreator(IDictionary<string, int> counts)
        {
            return (object sender, DataReceivedEventArgs e) =>
            {
                foreach (string key in counts.Keys.ToList())
                {
                    if (e.Data?.Contains(key) ?? false)
                    {
                        counts[key] -= 1;
                    }
                }
            };
        }

        internal static IConfiguration localsettings = new ConfigurationBuilder().AddJsonFile(Path.Combine(
            new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
            "local.settings.json")).Build();

        private static string GetFunctionsFileName()
        {
            string filepath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"npm\node_modules\azure-functions-core-tools\bin\func.exe")
                : @"/usr/bin/func";
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"Azure Functions Core Tools not found at {filepath}");
            }
            return filepath;
        }

        private static string GetPrefix()
        {
            return Path.Combine("bin", "Debug", "net6.0");
        }

        internal static string GetLogValue(object value)
        {
            return value.GetType().FullName + ":" + JsonConvert.SerializeObject(value);
        }

        // Draft
        internal static async Task<string> getCosmosDBValuesAsync(string id)
        {

            CosmosClient client = new CosmosClient(
                 connectionString: Environment.GetEnvironmentVariable("cosmosConnectionString")!
                );
            Container container = client.GetDatabase("database-id").GetContainer("container-id");

            // Get LINQ IQueryable object
            IOrderedQueryable<Data> queryable = container.GetItemLinqQueryable<Data>();

            // Construct LINQ query
            var matches = queryable
                .Where(p => p.id == id);

            // Convert to feed iterator
            using FeedIterator<Data> linqFeed = matches.ToFeedIterator();
            FeedResponse<Data> response = await linqFeed.ReadNextAsync();

            var item = response.FirstOrDefault(defaultValue: null);
            if (item != null)
            {
                foreach (KeyValuePair<string, string> entry in item.values)
                {
                    id += " " + entry.Key + " " + entry.Value;
                }
                return id;
            }

            return " ";
        }

    }
}