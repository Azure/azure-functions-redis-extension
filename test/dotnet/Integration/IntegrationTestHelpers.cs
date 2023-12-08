﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    internal static class IntegrationTestHelpers
    {
        internal const string connectionStringSetting = "redisConnectionString";

        internal static Process StartFunction(string functionName, int port)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = GetFunctionsFileName(),
                Arguments = $"start --verbose --functions {functionName} --port {port} --no-build --prefix {GetPrefix()}",
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            info.EnvironmentVariables["FUNCTIONS_RUNTIME_SCALE_MONITORING_ENABLED"] = "1";
            Process functionsProcess = new Process() { StartInfo = info };

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
                functionsProcess.Kill();
                throw new Exception("Azure Functions Host did not start");
            }
            if (!functionLoaded.Task.Wait(TimeSpan.FromMinutes(1)))
            {
                functionsProcess.Kill();
                throw new Exception($"Did not load Function {functionName}");
            }
            functionsProcess.OutputDataReceived -= hostStartupHandler;
            functionsProcess.OutputDataReceived -= functionLoadedHandler;

            // Ensure that the client name is correctly set
            string connectionString = RedisUtilities.ResolveConnectionString(localsettings, connectionStringSetting);
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            options.AllowAdmin = true;
            options.ClientName = nameof(IntegrationTestHelpers);
            IConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(options);
            ClientInfo[] clients = multiplexer.GetServers()[0].ClientList();
            if (!clients.Any(client => client.Name == "AzureFunctionsRedisExtension." + functionName))
            {
                functionsProcess.Kill();
                throw new Exception("Function client not found on redis server.");
            }

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

        internal static IConfiguration hostsettings = new ConfigurationBuilder().AddJsonFile(Path.Combine(
            new DirectoryInfo(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName,
            "host.json")).Build();

        private static string GetFunctionsFileName()
        {
            string filepath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Program Files\Microsoft\Azure Functions Core Tools\func.exe"
                //? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"npm\node_modules\azure-functions-core-tools\bin\func.exe")
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

        internal class ScaleStatus
        {
            public int vote;
            public int targetWorkerCount;
        }
    }
}
