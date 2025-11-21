using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using TestAPI.Interfaces;

namespace TestAPI.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IDatabase _database;

        public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
        {
            _redis = redis;
            _logger = logger;
            _database = redis.GetDatabase();
        }

        public async Task<T?> GetAsync<T>(string key)
            where T : class
        {
            try
            {
                var value = await _database.StringGetAsync(key);
                if (value.IsNullOrEmpty)
                {
                    _logger.LogDebug("Cache miss for key: {Key}", key);
                    return null;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(value!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving from Redis cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
            where T : class
        {
            try
            {
                var serialized = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expiration);
                _logger.LogDebug(
                    "Cached value for key: {Key} with expiration: {Expiration}",
                    key,
                    expiration
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting Redis cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
                _logger.LogDebug("Removed key from cache: {Key}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing from Redis cache for key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence in Redis cache for key: {Key}", key);
                return false;
            }
        }
    }
}
