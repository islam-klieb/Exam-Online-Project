using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using Exam_Online_API.Shared.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.UserAnswers.GetUserAnswerHistory
{
    public class GetUserAnswerHistoryHandler : IRequestHandler<GetUserAnswerHistoryQuery, GetUserAnswerHistoryResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetUserAnswerHistoryHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHybridCacheService _cache;

        public GetUserAnswerHistoryHandler(
            ApplicationDbContext context,
            ILogger<GetUserAnswerHistoryHandler> logger,
            IHttpContextAccessor httpContextAccessor,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _cache = cache;
        }

        public async Task<GetUserAnswerHistoryResponse> Handle(GetUserAnswerHistoryQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new BusinessLogicException("User not authenticated");

            try
            {
                string cacheKey = $"user:{userId}:exam-history:cursor={request.LastAttemptDate?.ToString("O") ?? "start"}:size={request.PageSize}";

                var cached = await _cache.GetAsync<GetUserAnswerHistoryResponse>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Cache hit for {CacheKey}", cacheKey);
                    return cached;
                }

                var query = _context.UserExams
                    .AsNoTracking()
                    .Include(ue => ue.Exam)
                        .ThenInclude(e => e.Category)
                    .Include(ue => ue.Exam)
                        .ThenInclude(e => e.Questions)
                    .Where(ue => ue.UserId == userId &&
                                 (ue.AttemptStatus == ExamAttemptStatus.Completed ||
                                  ue.AttemptStatus == ExamAttemptStatus.TimedOut))
                    .OrderByDescending(ue => ue.AttemptDate)
                    .AsQueryable();

                if (request.ExamId.HasValue)
                    query = query.Where(ue => ue.ExamId == request.ExamId.Value);

                if (request.CategoryId.HasValue)
                    query = query.Where(ue => ue.Exam.CategoryId == request.CategoryId.Value);

                query = Pagination<UserExam>.KeysetPagination(query, request.LastAttemptDate, request.PageSize);

                var userExams = await query.Take(request.PageSize).ToListAsync(cancellationToken);

                var history = new List<UserExamHistoryDto>();

                foreach (var userExam in userExams)
                {
                    var correctAnswers = await CalculateCorrectAnswers(userExam.Id, cancellationToken);
                    var totalQuestions = userExam.Exam.Questions.Count;
                    var isHighestScore = await IsHighestScore(userId, userExam.ExamId, userExam.Score, cancellationToken);

                    history.Add(new UserExamHistoryDto
                    {
                        UserExamId = userExam.Id,
                        ExamId = userExam.ExamId,
                        ExamTitle = userExam.Exam.Title,
                        ExamIcon = userExam.Exam.Icon,
                        CategoryTitle = userExam.Exam.Category.Title,
                        Score = userExam.Score,
                        CorrectAnswers = correctAnswers,
                        TotalQuestions = totalQuestions,
                        AttemptDate = userExam.AttemptDate,
                        DurationTaken = userExam.DurationTaken,
                        AttemptStatus = userExam.AttemptStatus,
                        IsHighestScore = isHighestScore
                    });
                }

                var nextCursor = history.LastOrDefault()?.AttemptDate;

                var response = new GetUserAnswerHistoryResponse
                {
                    History = history,
                    PageSize = request.PageSize,
                    HasNextPage = history.Count == request.PageSize,
                    NextCursor = nextCursor
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

                _logger.LogInformation("Cached GetUserAnswerHistory for user {UserId} under {CacheKey}", userId, cacheKey);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam history for user: {UserId}", userId);
                throw new BusinessLogicException($"Failed to retrieve exam history: {ex.Message}");
            }
        }

        private async Task<int> CalculateCorrectAnswers(Guid userExamId, CancellationToken cancellationToken)
        {
            var userAnswers = await _context.UserAnswers
                .Include(ua => ua.Question)
                    .ThenInclude(q => q.Choices)
                .Where(ua => ua.UserExamId == userExamId)
                .ToListAsync(cancellationToken);

            var questionGroups = userAnswers.GroupBy(ua => ua.QuestionId);
            var correctCount = 0;

            foreach (var group in questionGroups)
            {
                var question = group.First().Question;
                var userSelectedIds = group.Select(ua => ua.ChoiceId).OrderBy(id => id).ToList();
                var correctIds = question.Choices.Where(c => c.IsCorrect).Select(c => c.Id).OrderBy(id => id).ToList();

                if (userSelectedIds.SequenceEqual(correctIds))
                    correctCount++;
            }

            return correctCount;
        }

        private async Task<bool> IsHighestScore(string userId, Guid examId, int currentScore, CancellationToken cancellationToken)
        {
            var maxScore = await _context.UserExams
                .Where(ue => ue.UserId == userId &&
                            ue.ExamId == examId &&
                            (ue.AttemptStatus == ExamAttemptStatus.Completed ||
                             ue.AttemptStatus == ExamAttemptStatus.TimedOut))
                .MaxAsync(ue => (int?)ue.Score, cancellationToken);

            return maxScore == null || currentScore >= maxScore;
        }
    }
}
