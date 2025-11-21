namespace TestAPI.Configuration
{
    public class KafkaSettings
    {
        public const string SectionName = "Kafka";

        public bool Enabled { get; set; } = false;
        public string BootstrapServers { get; set; } = "localhost:9092";
        public string Topic { get; set; } = "math-operations";
    }
}
