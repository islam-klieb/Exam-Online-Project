namespace Exam_Online_API.Infrastructure.Services.CacheInvalidationService
{
    public interface ICacheInvalidationService
    {
        Task InvalidateCategoryCacheAsync();
        Task InvalidateExamCacheAsync();
        Task InvalidateQuestionCacheAsync();
        Task InvalidateUserCacheAsync();

    }

}
