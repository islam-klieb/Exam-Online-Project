using Exam_Online_API.Infrastructure.Services.HybridCacheService;

namespace Exam_Online_API.Infrastructure.Services.CacheInvalidationService
{
    public class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly IHybridCacheService _cache;

        public CacheInvalidationService(IHybridCacheService cache)
        {
            _cache = cache;
        }

        public async Task InvalidateCategoryCacheAsync()
        {
            await _cache.RemoveByPrefixAsync("categories");
        }

        public async Task InvalidateExamCacheAsync()
        {
            await _cache.RemoveByPrefixAsync("exams");
        }

        public async Task InvalidateQuestionCacheAsync()
        {
            await _cache.RemoveByPrefixAsync("questions");
        }

        public async Task InvalidateUserCacheAsync()
        {
            await _cache.RemoveByPrefixAsync("usre");
        }
    }
}
