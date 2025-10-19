using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Exams.GetExamsByCategory
{
    public class GetExamsByCategoryHandler : IRequestHandler<GetExamsByCategoryQuery, GetExamsByCategoryResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetExamsByCategoryHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetExamsByCategoryHandler(
            ApplicationDbContext context,
            ILogger<GetExamsByCategoryHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetExamsByCategoryResponse> Handle(GetExamsByCategoryQuery request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

            if (category == null)
                throw new NotFoundException($"Category with ID {request.CategoryId} not found");

            try
            {
                var now = DateTime.UtcNow;

                string cacheKey = $"exams:category:{request.CategoryId}:page={request.PageNumber}:size={request.PageSize}";

                var cached = await _cache.GetAsync<GetExamsByCategoryResponse>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return cached;
                }

                var query = _context.Exams
                    .AsNoTracking()
                    .Include(e => e.Questions)
                    .Where(e => e.CategoryId == request.CategoryId &&
                                e.IsActive &&
                                e.Status == ExamStatus.Active &&
                                e.StartDate <= now &&
                                e.EndDate >= now);

                var totalCount = await query.CountAsync(cancellationToken);
                query = query.OrderBy(e => e.Title);
                query = Pagination<Exam>.OffsetPagination(query, request.PageNumber, request.PageSize);

                var exams = await query
                    .Select(e => new ExamForUserDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Icon = e.Icon,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        Duration = e.Duration,
                        QuestionCount = e.Questions.Count
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var response = new GetExamsByCategoryResponse
                {
                    Exams = exams,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages,
                    CategoryTitle = category.Title
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Cached GetExamsByCategory result under {CacheKey}", cacheKey);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exams for category: {CategoryId}", request.CategoryId);
                throw new BusinessLogicException($"Failed to retrieve exams: {ex.Message}");
            }
        }
    }
}
