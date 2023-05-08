using Microsoft.Extensions.Hosting;

namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    internal class Program
    {
        public static void Main()
        {
            IHost host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}