using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestAPI.Configuration;
using TestAPI.Interfaces;
using TestAPI.Models;

namespace TestAPI.Services
{
    public class CachedCalculatorService : ICalculationService
    {
        private readonly ICalculationService _calculationService;
        private readonly ICacheService _cacheService;
        private readonly IMetadataService _metadataService;
        private readonly IEventPublisher _eventPublisher;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<CachedCalculatorService> _logger;

        public CachedCalculatorService(
            ICalculationService calculationService,
            ICacheService cacheService,
            IMetadataService metadataService,
            IEventPublisher eventPublisher,
            IOptions<CacheSettings> cacheSettings,
            ILogger<CachedCalculatorService> logger
        )
        {
            _calculationService = calculationService;
            _cacheService = cacheService;
            _metadataService = metadataService;
            _eventPublisher = eventPublisher;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        public Task<MathResponse> AddAsync(MathRequest request)
        {
            return ExecuteWithCacheAsync(
                "add",
                request,
                () => _calculationService.AddAsync(request)
            );
        }

        public Task<MathResponse> SubtractAsync(MathRequest request)
        {
            return ExecuteWithCacheAsync(
                "subtract",
                request,
                () => _calculationService.SubtractAsync(request)
            );
        }

        public Task<MathResponse> MultiplyAsync(MathRequest request)
        {
            return ExecuteWithCacheAsync(
                "multiply",
                request,
                () => _calculationService.MultiplyAsync(request)
            );
        }

        public Task<MathResponse> DivideAsync(MathRequest request)
        {
            return ExecuteWithCacheAsync(
                "divide",
                request,
                () => _calculationService.DivideAsync(request)
            );
        }

        private async Task<MathResponse> ExecuteWithCacheAsync(
            string operation,
            MathRequest request,
            Func<Task<MathResponse>> calculationFunc
        )
        {
            var cacheKey = $"{operation}:{request.X}:{request.Y}";

            // Try to get from cache
            var cachedResponse = await _cacheService.GetAsync<MathResponse>(cacheKey);
            if (cachedResponse != null)
            {
                _logger.LogInformation(
                    "Cache HIT - {Operation}({X},{Y}) = {Result}",
                    operation,
                    request.X,
                    request.Y,
                    cachedResponse.Result
                );
                cachedResponse.FromCache = true;
                return cachedResponse;
            }

            // Cache miss - fetch metadata
            _logger.LogInformation(
                "Cache MISS - {Operation}({X},{Y})",
                operation,
                request.X,
                request.Y
            );
            var metadata = await _metadataService.GetOperationMetadataAsync(operation);
            _logger.LogDebug("Metadata for {Operation}: {Metadata}", operation, metadata);

            // Perform calculation
            var response = await calculationFunc();

            // Cache the result
            await _cacheService.SetAsync(
                cacheKey,
                response,
                TimeSpan.FromSeconds(_cacheSettings.DurationSeconds)
            );

            // Publish event to Kafka
            await _eventPublisher.PublishCalculationEventAsync(
                operation,
                request.X,
                request.Y,
                response.Result
            );

            return response;
        }
    }
}
