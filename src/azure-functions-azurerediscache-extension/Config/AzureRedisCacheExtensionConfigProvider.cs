using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Microsoft.Azure.WebJobs.Extensions.AzureRedisCache
{

    [Extension("AzureRedisCache")]
    public class AzureRedisCacheExtensionConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _configuration;
        public AzureRedisCacheExtensionConfigProvider(IConfiguration configuration) 
        {
            _configuration = configuration;
        }

        ///<summary>
        ///Initializes binding to trigger provider via binding rule,
        ///also adds JSON serialization for multi-language support
        ///</summary>
        public void Initialize(ExtensionConfigContext context)
        {
            AzureRedisCacheTriggerProvider triggerBindingProvider = new AzureRedisCacheTriggerProvider(_configuration);
            var rule = context.AddBindingRule<AzureRedisCacheTriggerAttribute>();
            rule.BindToTrigger<AzureRedisCacheMessageModel>(triggerBindingProvider);
            rule.AddConverter<AzureRedisCacheMessageModel, string>(args => Encoding.UTF8.GetString(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(args))));
        }
    }
}
