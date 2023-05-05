using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests
{
    public class CustomType
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string Random { get; set; }
    }
}
