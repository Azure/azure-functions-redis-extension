﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    internal static class IntegrationTestHelpers
    {
        internal const string connectionStringSetting = "redisConnectionString";
        internal static int nextPort = 2000;

        internal static Process StartFunction(string functionName)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = GetFunctionsFileName(),
                Arguments = $"start --verbose --functions {functionName} --port {Interlocked.Increment(ref nextPort)} --no-build --prefix {GetPrefix()}",
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
                if (e.Data?.Contains("Host started") ?? false)
                {
                    hostStarted.SetResult(true);
                }
            }
            functionsProcess.OutputDataReceived += hostStartupHandler;

            TaskCompletionSource<bool> functionLoaded = new TaskCompletionSource<bool>();
            void functionLoadedHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data?.Contains("Generating 1 job function(s)") ?? false)
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

            return functionsProcess;
        }

        internal static Process StartRedis()
        {
            ProcessStartInfo info = GetRedisProcessStartInfo(Interlocked.Increment(ref nextPort));

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
                redisProcess.Kill();
                throw new Exception("Redis did not start");
            }
            return redisProcess;
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

        private static ProcessStartInfo GetRedisProcessStartInfo(int port)
        {
            ProcessStartInfo info = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"C:\Windows\System32\wsl.exe" : @"/usr/bin/redis-server",
                Arguments = $"{(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? @"/usr/bin/redis-server " : "")}--port {port} --notify-keyspace-events AKE",
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };

            return info;
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
