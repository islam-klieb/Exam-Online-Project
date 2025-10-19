using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetCategoryAnalytics
{
    public class GetCategoryAnalyticsHandler
        : IRequestHandler<GetCategoryAnalyticsQuery, GetCategoryAnalyticsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetCategoryAnalyticsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetCategoryAnalyticsHandler(
            ApplicationDbContext context,
            ILogger<GetCategoryAnalyticsHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetCategoryAnalyticsResponse> Handle(
            GetCategoryAnalyticsQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Categories
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var term = request.SearchTerm.Trim().ToLower();
                    query = query.Where(c => c.Title.ToLower().Contains(term));
                }

                var totalCount = await query.CountAsync(cancellationToken);
                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                query = query.OrderBy(c => c.Title);

                query = Pagination<Category>.OffsetPagination(query, request.PageNumber, request.PageSize);

                string cacheKey = $"categories:analytics:page={request.PageNumber}:size={request.PageSize}:search={request.SearchTerm ?? ""}";

                var data = await _cache.GetOrSetAsync(cacheKey, async () =>
                {
                    return await query
                        .Select(c => new CategoryAnalyticsDto
                        {
                            CategoryId = c.Id,
                            Title = c.Title,

                            ExamCount = _context.Exams.Count(e => e.CategoryId == c.Id),
                            QuestionCount = _context.Questions.Count(q => q.Exam.CategoryId == c.Id),
                            AttemptCount = _context.UserExams.Count(ue => ue.Exam.CategoryId == c.Id),

                            AverageScore = _context.UserExams
                                .Where(ue => ue.Exam.CategoryId == c.Id)
                                .Select(ue => (double?)ue.Score)
                                .Average()
                        })
                        .ToListAsync(cancellationToken);
                },
                TimeSpan.FromDays(1)); // Cache for 24h

                _logger.LogInformation("Retrieved {Count} category analytics records (Page {Page}/{Total})",
                    data.Count, request.PageNumber, totalPages);

                return new GetCategoryAnalyticsResponse
                {
                    Categories = data,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching category analytics");
                throw new BusinessLogicException($"Failed to get category analytics: {ex.Message}");
            }
        }
    }
}
