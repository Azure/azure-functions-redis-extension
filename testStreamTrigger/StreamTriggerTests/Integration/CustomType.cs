using System.Collections.Generic;

namespace Microsoft.Azure.WebJobs.Extensions.Redis.Tests.Integration
{
    public class CustomType
    {
        public string Name { get; set; }
        public string Field { get; set; }
        public string Random { get; set; }
    }

    public class Data
    {
        public string id { get; set; }
        public Dictionary<string, string> values { get; set; }
    }

}
