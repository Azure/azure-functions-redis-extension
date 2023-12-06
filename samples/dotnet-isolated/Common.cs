namespace Microsoft.Azure.Functions.Worker.Extensions.Redis.Samples
{
    public static class Common
    {
        public const string localhostSetting = "redisLocalhost";

        public class CustomType
        {
            public string Name { get; set; }
            public string Id { get; set; }
            public string Field { get; set; }
        }
    }
}
