using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Categories.GetCategoryDeletionImpact
{
    public class GetCategoryDeletionImpactHandler
        : IRequestHandler<GetCategoryDeletionImpactQuery, CategoryDeletionImpact>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetCategoryDeletionImpactHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetCategoryDeletionImpactHandler(
            ApplicationDbContext context,
            ILogger<GetCategoryDeletionImpactHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<CategoryDeletionImpact> Handle(GetCategoryDeletionImpactQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"category:deletion-impact:{request.Id}";

            var impact = await _cache.GetOrSetAsync < CategoryDeletionImpact > (cacheKey, async () =>
            {
                var category = await _context.Categories
                    .AsNoTracking()
                    .Where(c => c.Id == request.Id)
                    .Select(c => new { c.Id, c.Title })
                    .FirstOrDefaultAsync(cancellationToken);

                if (category == null)
                    throw new NotFoundException($"Category with ID {request.Id} not found.");

                var examCount = await _context.Exams.AsNoTracking()
                    .CountAsync(e => e.CategoryId == category.Id, cancellationToken);

                var questionCount = await _context.Questions.AsNoTracking()
                    .CountAsync(q => q.Exam.CategoryId == category.Id, cancellationToken);

                var choiceCount = await _context.Choices.AsNoTracking()
                    .CountAsync(c => c.Question.Exam.CategoryId == category.Id, cancellationToken);

                var userExamCount = await _context.UserExams.AsNoTracking()
                    .CountAsync(ue => ue.Exam.CategoryId == category.Id, cancellationToken);

                var userAnswerCount = await _context.UserAnswers.AsNoTracking()
                    .CountAsync(ua => ua.UserExam.Exam.CategoryId == category.Id, cancellationToken);

                var uniqueUsersAffected = await _context.UserExams.AsNoTracking()
                    .Where(ue => ue.Exam.CategoryId == category.Id)
                    .Select(ue => ue.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken);

                var hasActiveExams = await _context.Exams.AsNoTracking()
                    .AnyAsync(e => e.CategoryId == category.Id &&
                                   e.IsActive &&
                                   e.Status == ExamStatus.Active,
                              cancellationToken);

                var hasInProgressExams = await _context.UserExams.AsNoTracking()
                    .AnyAsync(ue => ue.Exam.CategoryId == category.Id &&
                                    ue.AttemptStatus == ExamAttemptStatus.InProgress,
                              cancellationToken);

                var examTitles = await _context.Exams.AsNoTracking()
                    .Where(e => e.CategoryId == category.Id)
                    .OrderByDescending(e => e.CreatedAt)
                    .Select(e => e.Title)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                string warningMessage = hasInProgressExams
                    ? "CRITICAL: Some users are currently taking exams in this category!"
                    : hasActiveExams
                        ? $"WARNING: This category contains {examCount} active exam(s) that will be deleted."
                        : uniqueUsersAffected > 0
                            ? $"This category has historical data for {uniqueUsersAffected} user(s). Their attempts will also be removed."
                            : "This category has no active or historical data.";

                var impact = new CategoryDeletionImpact
                {
                    CategoryId = category.Id,
                    CategoryTitle = category.Title,
                    ExamCount = examCount,
                    QuestionCount = questionCount,
                    ChoiceCount = choiceCount,
                    UserExamCount = userExamCount,
                    UserAnswerCount = userAnswerCount,
                    UniqueUsersAffected = uniqueUsersAffected,
                    ExamTitles = examTitles,
                    HasActiveExams = hasActiveExams,
                    HasInProgressUserExams = hasInProgressExams,
                    WarningMessage = warningMessage
                };

                _logger.LogInformation(
                    "Computed deletion impact for category {CategoryId} — cached for 10 minutes",
                    category.Id);

                return impact;

            }, TimeSpan.FromMinutes(10));
            return impact!;
        }

    }
}
