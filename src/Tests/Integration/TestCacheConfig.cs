using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Channels;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache.Tests
{
    /*
    JSON config file schema is:
    {
        "ConnectionString": <YOUR_CACHE_CONNECTION_STRING_HERE>,
        "ChannelName": <YOUR_CHANNEL_NAME_HERE>
    }
    */
    public class TestCacheConfig
    {
        ///<summary>
        ///Using to deserialize JSON object.
        ///</summary>
        private class jsonConfig
        {
            public string ConnectionString { get; set; }
            public string ChannelName { get; set; }
        }

        ///<summary>
        ///Reading config JSON from provided file URL into custom class.
        ///</summary>
        public TestCacheConfig(string fileURL)
        {
            using (StreamReader r = new StreamReader(fileURL))
            {
                string json = r.ReadToEnd();
                Console.WriteLine(json);
                jsonConfig currentConfig = JsonConvert.DeserializeObject<jsonConfig>(json);

                connectionString = currentConfig.ConnectionString;
                channelName = currentConfig.ChannelName;
            }
        }
        
        public string connectionString { get; set; }
        public string channelName { get; set; }

    }
}
