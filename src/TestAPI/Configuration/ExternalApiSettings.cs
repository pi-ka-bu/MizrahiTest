namespace TestAPI.Configuration
{
    public class ExternalApiSettings
    {
        public const string SectionName = "ExternalApi";

        public string MockoonBaseUrl { get; set; } = "http://localhost:3000";
        public int TimeoutSeconds { get; set; } = 30;
        public bool UseFallback { get; set; } = true;
    }
}
