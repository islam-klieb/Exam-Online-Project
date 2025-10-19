using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetDashboardStats
{
    public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, GetDashboardStatsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetDashboardStatsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetDashboardStatsHandler(ApplicationDbContext context, 
                                        ILogger<GetDashboardStatsHandler> logger,
                                        IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetDashboardStatsResponse> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = "dashboard:admin:stats";
            var cacheDuration = TimeSpan.FromSeconds(30);

            try
            {
                var cached = await _cache.GetAsync<GetDashboardStatsResponse>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Dashboard stats served from cache");
                    return cached;
                }
                var totalUsersTask = _context.Users.CountAsync(cancellationToken);
                var totalCategoriesTask = _context.Categories.CountAsync(cancellationToken);
                var totalExamsTask = _context.Exams.CountAsync(cancellationToken);
                var totalQuestionsTask = _context.Questions.CountAsync(cancellationToken);
                var totalAttemptsTask = _context.UserExams.CountAsync(cancellationToken);
                var activeExamsTask = _context.Exams.CountAsync(e => e.Status == ExamStatus.Active, cancellationToken);
                var completedExamsTask = _context.Exams.CountAsync(e => e.Status == ExamStatus.Completed, cancellationToken);
                var averageScoreTask = _context.UserExams.Any()
                    ? _context.UserExams.AverageAsync(ue => ue.Score, cancellationToken)
                    : Task.FromResult(0.0);

                await Task.WhenAll(
                    totalUsersTask,
                    totalCategoriesTask,
                    totalExamsTask,
                    totalQuestionsTask,
                    totalAttemptsTask,
                    activeExamsTask,
                    completedExamsTask,
                    averageScoreTask
                );

                var result = new GetDashboardStatsResponse
                {
                    TotalUsers = totalUsersTask.Result,
                    TotalCategories = totalCategoriesTask.Result,
                    TotalExams = totalExamsTask.Result,
                    TotalQuestions = totalQuestionsTask.Result,
                    TotalAttempts = totalAttemptsTask.Result,
                    ActiveExams = activeExamsTask.Result,
                    CompletedExams = completedExamsTask.Result,
                    AverageScore = Math.Round(averageScoreTask.Result, 2)
                };
                await _cache.SetAsync(cacheKey, result, cacheDuration);

                _logger.LogInformation("Dashboard stats cached for {Duration}s", cacheDuration.TotalSeconds);

                _logger.LogInformation("Dashboard stats retrieved successfully");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                throw new BusinessLogicException($"Failed to retrieve dashboard stats: {ex.Message}");
            }
        }
    }
}
