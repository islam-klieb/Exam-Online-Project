namespace Exam_Online_API.Infrastructure.Services.HybridCacheService
{
    public interface IHybridCacheService
    {
        Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? cacheTime = null);
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? cacheTime = null);
        Task RemoveAsync(string key);
        Task RemoveByPrefixAsync(string prefix);

    }
}
