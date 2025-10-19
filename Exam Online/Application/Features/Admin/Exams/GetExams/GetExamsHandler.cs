using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam_Online_API.Application.Features.Admin.Exams.GetExams
{
    public class GetExamsHandler : IRequestHandler<GetExamsQuery, GetExamsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetExamsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetExamsHandler(
            ApplicationDbContext context,
            ILogger<GetExamsHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetExamsResponse> Handle(GetExamsQuery request, CancellationToken cancellationToken)
        {
            
            try
            {
                string cacheKey = $"exams:page={request.PageNumber}:size={request.PageSize}" +
                                  $":search={request.SearchTerm ?? ""}" +
                                  $":sort={request.SortBy}:{request.SortDirection}" +
                                  $":category={request.CategoryId?.ToString() ?? "none"}";


                var cachedResponse = await _cache.GetAsync<GetExamsResponse>(cacheKey);
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Cache hit for exams list with key: {Key}", cacheKey);
                    return cachedResponse;
                } 


                var query = _context.Exams
                    .AsNoTracking()
                    .Include(e => e.Category)
                    .Include(e => e.Questions)
                    .AsQueryable();

                query = ApplyFilters(query, request);

                var totalCount = await query.CountAsync(cancellationToken);

                query = ApplySorting(query, request.SortBy, request.SortDirection);

                query = Pagination<Exam>.OffsetPagination(query,request.PageSize, request.PageSize);

                var exams = await query
                    .Select(e => new ExamDto
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Icon = e.Icon,
                        StartDate = e.StartDate,
                        EndDate = e.EndDate,
                        Duration = e.Duration,
                        Status = e.Status,
                        IsActive = e.IsActive,
                        CreatedAt = e.CreatedAt,
                        CategoryId = e.CategoryId,
                        CategoryTitle = e.Category.Title,
                        QuestionCount = e.Questions.Count
                    })
                    .ToListAsync(cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                var response = new GetExamsResponse
                {
                    Exams = exams,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };

                _logger.LogInformation("Retrieved {Count} exams (Page {Page}/{Total})", exams.Count, request.PageNumber, totalPages);

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exams");
                throw new BusinessLogicException($"Failed to retrieve exams: {ex.Message}");
            }
        }

        private IQueryable<Exam> ApplyFilters(IQueryable<Exam> query, GetExamsQuery request)
        {
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim().ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(searchTerm) ||
                    e.Id.ToString().Contains(searchTerm));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(e => e.CategoryId == request.CategoryId.Value);
            }

            if (request.StartDateFrom.HasValue)
            {
                query = query.Where(e => e.StartDate >= request.StartDateFrom.Value);
            }
            if (request.StartDateTo.HasValue)
            {
                query = query.Where(e => e.StartDate <= request.StartDateTo.Value);
            }

            if (request.EndDateFrom.HasValue)
            {
                query = query.Where(e => e.EndDate >= request.EndDateFrom.Value);
            }
            if (request.EndDateTo.HasValue)
            {
                query = query.Where(e => e.EndDate <= request.EndDateTo.Value);
            }

            if (request.DurationMin.HasValue)
            {
                query = query.Where(e => e.Duration >= request.DurationMin.Value);
            }
            if (request.DurationMax.HasValue)
            {
                query = query.Where(e => e.Duration <= request.DurationMax.Value);
            }

            return query;
        }

        private IQueryable<Exam> ApplySorting(
            IQueryable<Exam> query,
            ExamSortBy sortBy,
            SortDirection sortDirection)
        {
            Expression<Func<Exam, object>> sortExpression = sortBy switch
            {
                ExamSortBy.Title => e => e.Title,
                ExamSortBy.StartDate => e => e.StartDate,
                ExamSortBy.EndDate => e => e.EndDate,
                ExamSortBy.Duration => e => e.Duration,
                ExamSortBy.CreatedDate => e => e.CreatedAt,
                _ => e => e.Title
            };

            return sortDirection == SortDirection.Ascending
                ? query.OrderBy(sortExpression)
                : query.OrderByDescending(sortExpression);
        }
    }
}
