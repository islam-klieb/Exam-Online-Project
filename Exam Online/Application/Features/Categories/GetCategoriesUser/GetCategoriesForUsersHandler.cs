using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Categories.GetCategoriesUser
{
    public class GetCategoriesForUsersHandler
        : IRequestHandler<GetCategoriesForUsersQuery, GetCategoriesForUsersResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetCategoriesForUsersHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetCategoriesForUsersHandler(
            ApplicationDbContext context,
            ILogger<GetCategoriesForUsersHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetCategoriesForUsersResponse> Handle(
            GetCategoriesForUsersQuery request,
            CancellationToken cancellationToken)
        {
            try
            {
                var now = DateTime.UtcNow;

                string cacheKey = $"categories:users:page={request.PageNumber}:size={request.PageSize}";

                var cachedResponse = await _cache.GetAsync<GetCategoriesForUsersResponse>(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return cachedResponse;
                }

                var query = _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Exams.Any(e =>
                        e.IsActive &&
                        e.Status == ExamStatus.Active &&
                        e.StartDate <= now &&
                        e.EndDate >= now));

                var totalCount = await query.CountAsync(cancellationToken);

                query = query.OrderBy(c => c.Title);
                query = Pagination<Category>.OffsetPagination(query, request.PageNumber, request.PageSize);

                var categories = await query
                    .Select(c => new CategoryForUserDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Icon = c.Icon,
                        ActiveExamCount = c.Exams.Count(e =>
                            e.IsActive &&
                            e.Status == ExamStatus.Active &&
                            e.StartDate <= now &&
                            e.EndDate >= now)
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var response = new GetCategoriesForUsersResponse
                {
                    Categories = categories,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Categories cached successfully under key: {Key}", cacheKey);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories for users");
                throw new BusinessLogicException($"Failed to retrieve categories: {ex.Message}");
            }
        }
    }
}
