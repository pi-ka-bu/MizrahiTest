namespace TestAPI.Configuration
{
    public class CacheSettings
    {
        public const string SectionName = "Cache";

        public CacheType Type { get; set; } = CacheType.Memory;
        public int DurationSeconds { get; set; } = 30;
        public string? RedisConnectionString { get; set; }
    }

    public enum CacheType
    {
        Memory,
        Redis,
    }
}
