using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam_Online_API.Application.Features.Admin.Categories.GetCategories
{
    public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, GetCategoriesResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetCategoriesHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetCategoriesHandler(
            ApplicationDbContext context,
            ILogger<GetCategoriesHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetCategoriesResponse> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string normalizedSearch = request.SearchTerm?.Trim().ToLower() ?? string.Empty;
                string cacheKey = $"categories:page={request.PageNumber}:size={request.PageSize}:search={normalizedSearch}:sort={request.SortBy}:{request.SortDirection}";

                var cachedResult = await _cache.GetAsync<GetCategoriesResponse>(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return cachedResult;
                }

                var query = _context.Categories.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(normalizedSearch))
                    query = query.Where(c => c.Title.ToLower().Contains(normalizedSearch));

                var totalCount = await query.CountAsync(cancellationToken);
                query = ApplySorting(query, request.SortBy, request.SortDirection);
                query = Pagination<Category>.OffsetPagination(query, request.PageNumber, request.PageSize);

                var categories = await query
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Title = c.Title,
                        Icon = c.Icon,
                        CreatedAt = c.CreatedAt,
                        ExamCount = c.Exams.Count
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var response = new GetCategoriesResponse
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

                _logger.LogInformation("Cached category list with key {CacheKey}", cacheKey);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                throw new BusinessLogicException($"Failed to retrieve categories: {ex.Message}");
            }
        }

        private IQueryable<Category> ApplySorting(
            IQueryable<Category> query,
            CategorySortBy sortBy,
            SortDirection sortDirection)
        {
            Expression<Func<Category, object>> sortExpression = sortBy switch
            {
                CategorySortBy.Name => c => c.Title,
                CategorySortBy.CreatedDate => c => c.CreatedAt,
                _ => c => c.Title
            };

            return sortDirection == SortDirection.Ascending
                ? query.OrderBy(sortExpression)
                : query.OrderByDescending(sortExpression);
        }
    }
}
