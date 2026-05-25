using StackExchange.Redis;
using UrlShortener.Core.Services;
using Microsoft.Extensions.Logging;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key is required", nameof(key));

        try
        {
            bool success;
            if (expiry.HasValue)
                success = await _db.StringSetAsync(key, value, expiry.Value);
            else
                success = await _db.StringSetAsync(key, value);

            if (!success)
            {
                _logger.LogWarning("Redis failed to set key {Key}", key);
            }
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while setting key {Key}", key);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while setting key {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while setting key {Key}", key);
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key is required", nameof(key));

        try
        {
            var result = await _db.StringGetAsync(key);

            if (result.IsNullOrEmpty)
            {
                _logger.LogDebug("Cache miss for key {Key}", key);
                return null;
            }

            _logger.LogDebug("Cache hit for key {Key}", key);
            return result.ToString();
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while getting key {Key}", key);
            return null;
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogError(ex, "Redis connection error while getting key {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting key {Key}", key);
            return null;
        }
    }
}
