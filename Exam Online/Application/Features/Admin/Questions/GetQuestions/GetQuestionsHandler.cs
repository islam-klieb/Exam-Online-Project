using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Exam_Online_API.Application.Features.Admin.Questions.GetQuestions
{
    public class GetQuestionsHandler : IRequestHandler<GetQuestionsQuery, GetQuestionsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetQuestionsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetQuestionsHandler(
            ApplicationDbContext context,
            ILogger<GetQuestionsHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetQuestionsResponse> Handle(GetQuestionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                string cacheKey = $"questions:list:" +
                                  $"search={request.SearchTerm ?? ""}:" +
                                  $"sort={request.SortBy}-{request.SortDirection}:" +
                                  $"cursor={request.LastCreatedAt?.Ticks ?? 0}:" +
                                  $"size={request.PageSize}";

                var cached = await _cache.GetAsync<GetQuestionsResponse>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Cache hit for GetQuestions (Key={Key})", cacheKey);
                    return cached;
                }

                var query = _context.Questions
                    .AsNoTracking()
                    .Include(q => q.Exam)
                    .Include(q => q.Choices)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    var term = request.SearchTerm.Trim().ToLower();
                    query = query.Where(q => q.Title.ToLower().Contains(term));
                }

                query = ApplySorting(query, request.SortBy, request.SortDirection);

                query = Pagination<Question>.KeysetPagination(query, request.LastCreatedAt, request.SortDirection, request.PageSize);

                var questions = await query
                    .Select(q => new QuestionDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Type = q.Type,
                        ExamTitle = q.Exam.Title,
                        ChoiceCount = q.Choices.Count,
                        CreatedAt = q.CreatedAt
                    })
                    .ToListAsync(cancellationToken);

                var hasNext = questions.Count > request.PageSize;
                var resultList = questions.Take(request.PageSize).ToList();
                var nextCursor = hasNext ? (DateTime?)resultList.Last().CreatedAt : null;

                var response = new GetQuestionsResponse
                {
                    Questions = resultList,
                    PageSize = request.PageSize,
                    HasNextPage = hasNext,
                    NextCursor = nextCursor
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(3));

                _logger.LogInformation(
                    "Questions retrieved and cached successfully (Count={Count})",
                    resultList.Count);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving questions");
                throw new BusinessLogicException($"Failed to retrieve questions: {ex.Message}");
            }
        }

        private IQueryable<Question> ApplySorting(
            IQueryable<Question> query,
            QuestionSortBy sortBy,
            SortDirection sortDirection)
        {
            Expression<Func<Question, object>> sortExpression = sortBy switch
            {
                QuestionSortBy.Title => q => q.Title,
                QuestionSortBy.CreatedDate => q => q.CreatedAt,
                _ => q => q.CreatedAt
            };

            return sortDirection == SortDirection.Ascending
                ? query.OrderBy(sortExpression)
                : query.OrderByDescending(sortExpression);
        }
    }
}
