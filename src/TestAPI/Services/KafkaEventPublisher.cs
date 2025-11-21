using System;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestAPI.Configuration;
using TestAPI.Interfaces;

namespace TestAPI.Services
{
    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<Null, string>? _producer;
        private readonly KafkaSettings _settings;
        private readonly ILogger<KafkaEventPublisher> _logger;

        public KafkaEventPublisher(
            IOptions<KafkaSettings> settings,
            ILogger<KafkaEventPublisher> logger
        )
        {
            _settings = settings.Value;
            _logger = logger;

            if (_settings.Enabled)
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = _settings.BootstrapServers,
                    ClientId = "math-api-producer",
                };

                try
                {
                    _producer = new ProducerBuilder<Null, string>(config).Build();
                    _logger.LogInformation(
                        "Kafka producer initialized with bootstrap servers: {Servers}",
                        _settings.BootstrapServers
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to initialize Kafka producer. Events will not be published."
                    );
                    _producer = null;
                }
            }
            else
            {
                _logger.LogInformation("Kafka publishing is disabled");
            }
        }

        public async Task PublishCalculationEventAsync(
            string operation,
            decimal x,
            decimal y,
            decimal result
        )
        {
            if (!_settings.Enabled || _producer == null)
            {
                _logger.LogDebug(
                    "Kafka is disabled or producer not available, skipping event publish"
                );
                return;
            }

            try
            {
                var eventData = new
                {
                    requestId = Guid.NewGuid(),
                    operation,
                    x,
                    y,
                    result,
                    timestamp = DateTime.UtcNow,
                };

                var message = new Message<Null, string>
                {
                    Value = JsonSerializer.Serialize(eventData),
                };

                var deliveryResult = await _producer.ProduceAsync(_settings.Topic, message);
                _logger.LogInformation(
                    "Event published to Kafka topic {Topic}: {Operation}({X},{Y})={Result}, Offset: {Offset}",
                    _settings.Topic,
                    operation,
                    x,
                    y,
                    result,
                    deliveryResult.Offset
                );
            }
            catch (ProduceException<Null, string> ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish event to Kafka: {ErrorReason}",
                    ex.Error.Reason
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error publishing event to Kafka");
            }
        }

        public void Dispose()
        {
            try
            {
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.LogInformation("Kafka producer disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Kafka producer");
            }
        }
    }
}
