namespace Microsoft.Azure.WebJobs.Extensions.Redis.Samples
{
    public static class Common
    {
        public const string connectionStringSetting = "redisConnectionString";

        public class CustomType
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Field { get; set; }
        }
    }
}
