using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestAPI.Configuration;
using TestAPI.Interfaces;

namespace TestAPI.Services
{
    public class MetadataService : IMetadataService
    {
        private readonly HttpClient _httpClient;
        private readonly ExternalApiSettings _settings;
        private readonly ILogger<MetadataService> _logger;

        private static readonly Dictionary<string, string> FallbackMetadata = new()
        {
            { "add", "Addition operation: combines two numbers" },
            { "subtract", "Subtraction operation: finds the difference between two numbers" },
            { "multiply", "Multiplication operation: calculates the product of two numbers" },
            { "divide", "Division operation: calculates the quotient of two numbers" },
        };

        public MetadataService(
            HttpClient httpClient,
            IOptions<ExternalApiSettings> settings,
            ILogger<MetadataService> logger
        )
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);
        }

        public async Task<string> GetOperationMetadataAsync(string operation)
        {
            try
            {
                var url = $"{_settings.MockoonBaseUrl}/api/meta/{operation}";
                _logger.LogDebug("Fetching metadata from: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var metadata = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(
                    "Successfully retrieved metadata for operation: {Operation}",
                    operation
                );
                return metadata;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(
                    ex,
                    "HTTP request failed when fetching metadata for operation: {Operation}",
                    operation
                );
                return GetFallbackMetadata(operation);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Request timeout when fetching metadata for operation: {Operation}",
                    operation
                );
                return GetFallbackMetadata(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error fetching metadata for operation: {Operation}",
                    operation
                );
                return GetFallbackMetadata(operation);
            }
        }

        private string GetFallbackMetadata(string operation)
        {
            if (_settings.UseFallback && FallbackMetadata.TryGetValue(operation, out var fallback))
            {
                _logger.LogInformation(
                    "Using fallback metadata for operation: {Operation}",
                    operation
                );
                return fallback;
            }

            _logger.LogWarning(
                "No fallback metadata available for operation: {Operation}",
                operation
            );
            return $"Metadata not available for operation: {operation}";
        }
    }
}
