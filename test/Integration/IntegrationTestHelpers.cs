using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    internal static class IntegrationTestHelpers
    {
        internal static Process StartFunction(string functionName, int port)
        {
            Process functionsProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = GetFunctionsFileName(),
                    Arguments = $"start --verbose --functions {functionName} --port {port}",
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
                if (e.Data.Contains($"Host started"))
                {
                    hostStarted.SetResult(true);
                }
            }
            functionsProcess.OutputDataReceived += hostStartupHandler;

            TaskCompletionSource<bool> functionLoaded = new TaskCompletionSource<bool>();
            void functionLoadedHandler(object sender, DataReceivedEventArgs e)
            {
                if (e.Data.Contains($"Generating 1 job function(s)"))
                {
                    functionLoaded.SetResult(true);
                }
            }
            functionsProcess.OutputDataReceived += functionLoadedHandler;



            functionsProcess.Start();
            functionsProcess.BeginOutputReadLine();
            functionsProcess.BeginErrorReadLine();
            if (!hostStarted.Task.Wait(TimeSpan.FromSeconds(60)))
            {
                throw new Exception("Azure Functions Host did not start");
            }
            if (!functionLoaded.Task.Wait(TimeSpan.FromSeconds(60)))
            {
                throw new Exception($"Did not load Function {functionName}");
            }
            functionsProcess.OutputDataReceived -= hostStartupHandler;
            functionsProcess.OutputDataReceived -= functionLoadedHandler;

            return functionsProcess;
        }

        internal static DataReceivedEventHandler CounterHandlerCreator(Dictionary<string, int> counts, TaskCompletionSource<bool> functionExecuted)
        {
            return (object sender, DataReceivedEventArgs e) =>
            {
                foreach (string key in counts.Keys.ToList())
                if (e.Data.Contains(key))
                {
                    counts[key] -= 1;
                    if (counts.Values.Sum() == 0)
                    {
                        functionExecuted.SetResult(true);
                    }
                }

            };
        }

        private static string GetFunctionsFileName()
        {
            string filepath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"npm\node_modules\azure-functions-core-tools\bin\func.exe")
                : @"/usr/local/lib/node_modules/azure-functions-core-tools/bin/func";
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"Azure Functions Core Tools not found at {filepath}");
            }
            return filepath;
        }
    }
}
