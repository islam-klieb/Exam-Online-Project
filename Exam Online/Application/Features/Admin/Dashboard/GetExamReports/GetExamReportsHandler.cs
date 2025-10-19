using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetExamReports
{
    public class GetExamReportsHandler : IRequestHandler<GetExamReportsQuery, GetExamReportsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetExamReportsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetExamReportsHandler(ApplicationDbContext context,
                                     ILogger<GetExamReportsHandler> logger,
                                     IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetExamReportsResponse> Handle(GetExamReportsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var query = _context.Exams
                    .AsNoTracking()   
                    .Include(e => e.Category)
                    .Include(e => e.Questions)
                    .Include(e => e.UserExams)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var term = request.SearchTerm.Trim().ToLower();
                    query = query.Where(e =>
                        e.Title.ToLower().Contains(term) ||
                        e.Category.Title.ToLower().Contains(term));
                }

                var totalCount = await query.CountAsync(cancellationToken);

                query = ApplySorting(query, request.SortBy, request.SortDirection);

                query = Pagination<Exam>.OffsetPagination(query,request.PageNumber,request.PageSize);

                string cacheKey = $"exams:reports:page={request.PageNumber}:size={request.PageSize}:search={request.SearchTerm ?? ""}";

                var exams = await _cache.GetOrSetAsync(cacheKey,async() => 
                { 
                    return await query
                    .Select(e => new ExamReportDto
                    {
                        ExamId = e.Id,
                        Title = e.Title,
                        Category = e.Category.Title,
                        Status = e.Status,
                        TotalQuestions = e.Questions.Count,
                        TotalAttempts = e.UserExams.Count,
                        AverageScore = e.UserExams.Any()
                            ? Math.Round(e.UserExams.Average(ue => ue.Score), 2)
                            : 0,
                        HighestScore = e.UserExams.Any()
                            ? e.UserExams.Max(ue => ue.Score)
                            : 0,
                        LowestScore = e.UserExams.Any()
                            ? e.UserExams.Min(ue => ue.Score)
                            : 0,
                        CreatedAt = e.CreatedAt
                    })
                    .ToListAsync(cancellationToken);
                },TimeSpan.FromDays(1));

                

                var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

                _logger.LogInformation("Exam reports retrieved ({Count} exams)", exams!.Count);

                return new GetExamReportsResponse
                {
                    Exams = exams,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam reports");
                throw new BusinessLogicException($"Failed to retrieve exam reports: {ex.Message}");
            }
        }

        private IQueryable<Exam> ApplySorting(
            IQueryable<Exam> query,
            ExamSortBy sortBy,
            SortDirection direction)
        {
            Expression<Func<Exam, object>> keySelector = sortBy switch
            {
                ExamSortBy.Title => e => e.Title,
                ExamSortBy.CreatedDate => e => e.CreatedAt,
                _ => e => e.CreatedAt
            };

            return direction == SortDirection.Ascending
                ? query.OrderBy(keySelector)
                : query.OrderByDescending(keySelector);
        }
    }
}
