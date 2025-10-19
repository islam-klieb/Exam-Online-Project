using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using System.Text.Json;

public class HybridCacheService : IHybridCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly ILogger<HybridCacheService> _logger;

    public HybridCacheService(IMemoryCache memoryCache, IConnectionMultiplexer redis, ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _redis = redis;
        _db = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? cacheTime = null)
    {
        cacheTime ??= TimeSpan.FromMinutes(5);

        if (_memoryCache.TryGetValue(key, out T? memoryValue))
        {
            _logger.LogDebug("Memory cache hit: {Key}", key);
            return memoryValue;
        }

        var redisValue = await _db.StringGetAsync(key);

        if (redisValue.HasValue)
        {
            var data = JsonSerializer.Deserialize<T>(redisValue!);
            _memoryCache.Set(key, data, cacheTime.Value);
            _logger.LogDebug("Redis cache hit: {Key}", key);
            return data;
        }

        var result = await factory();
        if (result == null)
        {
            _logger.LogWarning("Factory returned null for key: {Key}", key);
            return default;
        }

        var serialized = JsonSerializer.Serialize(result);
        await _db.StringSetAsync(key, serialized, cacheTime.Value);

        await _db.SetAddAsync("cache:keys", key);

        _logger.LogInformation("Cached new value: {Key} for {Minutes}m", key, cacheTime.Value.TotalMinutes);
        return result;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
            return memoryValue;

        var redisValue = await _db.StringGetAsync(key);
        if (redisValue.HasValue)
        {
            var data = JsonSerializer.Deserialize<T>(redisValue!);
            _memoryCache.Set(key, data, TimeSpan.FromMinutes(5));
            return data;
        }

        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? cacheTime = null)
    {
        cacheTime ??= TimeSpan.FromMinutes(5);
        var serialized = JsonSerializer.Serialize(value);

        await _db.StringSetAsync(key, serialized, cacheTime.Value);
        await _db.SetAddAsync("cache:keys", key);

        _logger.LogDebug("Cache set: {Key}", key);
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _db.KeyDeleteAsync(key);
        _logger.LogDebug("Cache removed: {Key}", key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());

        var keys = server.Keys(pattern: $"{prefix}:*").ToArray();
        foreach (var key in keys)
        {
            await _db.KeyDeleteAsync(key);
            _memoryCache.Remove(key.ToString());
        }

        _logger.LogInformation("Removed {Count} cache entries with prefix: {Prefix}", keys.Length, prefix);
    }
}
