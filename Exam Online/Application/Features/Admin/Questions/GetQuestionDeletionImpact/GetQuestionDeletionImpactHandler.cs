using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Questions.GetQuestionDeletionImpact
{
    public class GetQuestionDeletionImpactHandler
        : IRequestHandler<GetQuestionDeletionImpactQuery, QuestionDeletionImpact>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetQuestionDeletionImpactHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetQuestionDeletionImpactHandler(
            ApplicationDbContext context,
            ILogger<GetQuestionDeletionImpactHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<QuestionDeletionImpact> Handle(
            GetQuestionDeletionImpactQuery request,
            CancellationToken cancellationToken)
        {
            string cacheKey = $"question:deletionImpact:{request.Id}";

            var cached = await _cache.GetAsync<QuestionDeletionImpact>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Cache hit for question deletion impact (ID={QuestionId})", request.Id);
                return cached;
            }

            var question = await _context.Questions
                .AsNoTracking()
                .Where(q => q.Id == request.Id)
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    q.Type,
                    q.ExamId,
                    ExamTitle = q.Exam.Title,
                    CategoryTitle = q.Exam.Category.Title,
                    q.Exam.IsActive,
                    q.Exam.Status
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (question == null)
                throw new NotFoundException($"Question with ID {request.Id} not found.");

            var choiceCount = await _context.Choices
                .CountAsync(c => c.QuestionId == question.Id, cancellationToken);

            var choiceImagesCount = await _context.Choices
                .CountAsync(c => c.QuestionId == question.Id &&
                                 c.ChoiceType == FileType.Image &&
                                 !string.IsNullOrEmpty(c.ChoiceFilePath),
                            cancellationToken);

            var userAnswerCount = await _context.UserAnswers
                .CountAsync(ua => ua.QuestionId == question.Id, cancellationToken);

            var uniqueUsers = await _context.UserAnswers
                .Where(ua => ua.QuestionId == question.Id)
                .Select(ua => ua.UserExam.UserId)
                .Distinct()
                .CountAsync(cancellationToken);

            bool hasUserAnswers = userAnswerCount > 0;
            bool examIsActive = question.IsActive && question.Status == ExamStatus.Active;

            string warningMessage = examIsActive
                ? "This question belongs to an active exam. Deleting it may affect ongoing attempts."
                : hasUserAnswers
                    ? $"This question has been answered by {uniqueUsers} user(s). Historical records will remain."
                    : "This question has no recorded user answers.";

            var impact = new QuestionDeletionImpact
            {
                QuestionId = question.Id,
                QuestionTitle = question.Title,
                ExamTitle = question.ExamTitle,
                CategoryTitle = question.CategoryTitle,
                QuestionType = question.Type,
                ChoiceCount = choiceCount,
                UserAnswerCount = userAnswerCount,
                UniqueUsersAffected = uniqueUsers,
                ChoiceImagesCount = choiceImagesCount,
                ExamIsActive = examIsActive,
                HasUserAnswers = hasUserAnswers,
                WarningMessage = warningMessage
            };

            await _cache.SetAsync(cacheKey, impact, TimeSpan.FromMinutes(5));

            _logger.LogInformation(
                "Deletion impact computed and cached for question {QuestionId}", question.Id);

            return impact;
        }
    }
}
