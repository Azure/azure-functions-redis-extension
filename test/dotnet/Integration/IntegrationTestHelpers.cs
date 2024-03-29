﻿using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
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
        internal const int PollingIntervalShort = 100;
        internal const int PollingIntervalLong = 10000;
        internal const int BatchSize = 10;
        internal const string format = "triggerValue:{0}";
        internal const string PubSubChannel = "testChannel";
        internal const string PubSubMultiple = "testChannel*";
        internal const string KeyspaceChannel = "__keyspace@0__:testKey";
        internal const string KeyspaceMultiple = "__keyspace@0__:testKey*";
        internal const string KeyeventChannelSet = "__keyevent@0__:set";
        internal const string KeyeventChannelAll = "__keyevent@0__:*";
        internal const string KeyspaceChannelAll = "__keyspace@0__:*";
        internal const string AllChannels = "*";

        internal const string ConnectionString = "redisConnectionString";
        internal const string ManagedIdentity = "redisManagedIdentity";
        internal const string Redis60 = "/redis/redis-6.0.20";
        internal const string Redis62 = "/redis/redis-6.2.14";
        internal const string Redis70 = "/redis/redis-7.0.14";

        internal static async Task<Process> StartFunctionAsync(string functionName, int port, bool managedIdentity = false)
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
            ConfigurationOptions options = await RedisUtilities.ResolveConfigurationOptionsAsync(localsettings, new ClientSecretCredentialComponentFactory(), managedIdentity ? ManagedIdentity : ConnectionString, nameof(IntegrationTestHelpers));
            options.AllowAdmin = true;
            IConnectionMultiplexer multiplexer = await ConnectionMultiplexer.ConnectAsync(options);
            ClientInfo[] clients = multiplexer.GetServers()[0].ClientList();
            if (!clients.Any(client => client.Name == RedisUtilities.GetRedisClientName(functionName)))
            {
                functionsProcess.Kill();
                throw new Exception("Function client not found on redis server.");
            }

            return functionsProcess;
        }

        internal static Process StartRedis(string versionPath)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Windows\System32\wsl.exe" : $"{versionPath}/src/redis-server",
                Arguments = $"{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"{versionPath}/src/redis-server " : "")}--port 6379 --notify-keyspace-events AKE",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            Process redisProcess = new Process() { StartInfo = info };

            TaskCompletionSource<bool> hostStarted = new TaskCompletionSource<bool>();
            void hostStartupHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data?.Contains("* Ready to accept connections") ?? false)
                {
                    hostStarted.SetResult(true);
                }
            }
            redisProcess.OutputDataReceived += hostStartupHandler;

            redisProcess.Start();
            redisProcess.BeginOutputReadLine();
            redisProcess.BeginErrorReadLine();
            if (!hostStarted.Task.Wait(TimeSpan.FromMinutes(1)))
            {
                StopRedis(redisProcess);
                throw new Exception("Redis did not start");
            }
            return redisProcess;
        }

        internal static void StopRedis(Process redis)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                redis.Kill();
            }
            else
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\wsl.exe",
                    Arguments = "pkill redis-server",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                };
                Process redisKillProcess = new Process() { StartInfo = info };
                redisKillProcess.Start();
                redisKillProcess.WaitForExit();
            }
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
                ? GetWindowsFunctionsFilePath()
                : @"/usr/bin/func"; 
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"Azure Functions Core Tools not found at {filepath}");
            }
            return filepath;
        }

        private static string GetWindowsFunctionsFilePath()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c where func",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string filepath = proc.StandardOutput.ReadLine();
            proc.WaitForExit();
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

        internal class ClientSecretCredentialComponentFactory : AzureComponentFactory
        {
            public override object CreateClient(Type clientType, IConfiguration configuration, TokenCredential credential, object clientOptions)
            {
                throw new NotImplementedException();
            }

            public override object CreateClientOptions(Type optionsType, object serviceVersion, IConfiguration configuration)
            {
                throw new NotImplementedException();
            }

            public override TokenCredential CreateTokenCredential(IConfiguration configuration)
            {
                var clientId = configuration["clientId"];
                var tenantId = configuration["tenantId"];
                var clientSecret = configuration["clientSecret"];
                return new ClientSecretCredential(tenantId, clientId, clientSecret);
            }
        }
    }
}
