using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Exams.GetExamDeletionImpact
{
    public class GetExamDeletionImpactHandler
         : IRequestHandler<GetExamDeletionImpactQuery, ExamDeletionImpact>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GetExamDeletionImpactHandler> _logger;
        private readonly IHybridCacheService _cache;
        public GetExamDeletionImpactHandler(
            ApplicationDbContext context,
            ILogger<GetExamDeletionImpactHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ExamDeletionImpact> Handle(GetExamDeletionImpactQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"exams:Impact";

            var data = await _cache.GetOrSetAsync(cacheKey, async () =>
            {
                var exam = await _context.Exams
                .AsNoTracking()
                .Where(e => e.Id == request.Id)
                .Select(e => new
                {
                    e.Id,
                    e.Title,
                    CategoryTitl = e.Category.Title,
                    e.CategoryId,
                    e.IsActive,
                    e.Status
                })
                .FirstOrDefaultAsync(cancellationToken);

                if (exam == null)
                    throw new NotFoundException($"Exam with ID {request.Id} not found.");

                var questionCount = await _context.Questions
                    .CountAsync(q => q.ExamId == exam.Id, cancellationToken);

                var choiceCount = await _context.Choices
                    .CountAsync(c => c.Question.ExamId == exam.Id, cancellationToken);

                var userExamCount = await _context.UserExams
                    .CountAsync(ue => ue.ExamId == exam.Id, cancellationToken);

                var userAnswerCount = await _context.UserAnswers
                    .CountAsync(ua => ua.UserExam.ExamId == exam.Id, cancellationToken);

                var uniqueUsers = await _context.UserExams
                    .Where(ue => ue.ExamId == exam.Id)
                    .Select(ue => ue.UserId)
                    .Distinct()
                    .CountAsync(cancellationToken);

                var hasActiveAttempts = await _context.UserExams
                    .AnyAsync(ue => ue.ExamId == exam.Id &&
                                    ue.AttemptStatus == ExamAttemptStatus.InProgress,
                              cancellationToken);

                var choiceImagesCount = await _context.Choices
                    .Where(c => c.Question.ExamId == exam.Id &&
                                c.ChoiceType == FileType.Image &&
                                !string.IsNullOrEmpty(c.ChoiceFilePath))
                    .CountAsync(cancellationToken);

                var questionTitles = await _context.Questions
                    .Where(q => q.ExamId == exam.Id)
                    .OrderByDescending(q => q.CreatedAt)
                    .Select(q => q.Title)
                    .Take(5)
                    .ToListAsync(cancellationToken);

                string warningMessage = hasActiveAttempts
                    ? "CRITICAL: Some users are currently taking this exam!"
                    : exam.IsActive && exam.Status == ExamStatus.Active
                        ? "WARNING: This is an active exam. Students can currently access it."
                        : uniqueUsers > 0
                            ? $"This exam has historical attempts from {uniqueUsers} user(s). Deleting it will remove their results."
                            : "This exam has no user attempts yet.";

                _logger.LogInformation(
                    "Deletion impact for exam {ExamId}: Questions={Questions}, Users={Users}, Active={Active}",
                    exam.Id, questionCount, uniqueUsers, exam.IsActive);

                return new ExamDeletionImpact
                {
                    ExamId = exam.Id,
                    ExamTitle = exam.Title,
                    CategoryTitle = exam.CategoryTitl,
                    QuestionCount = questionCount,
                    ChoiceCount = choiceCount,
                    UserExamCount = userExamCount,
                    UserAnswerCount = userAnswerCount,
                    UniqueUsersAffected = uniqueUsers,
                    ChoiceImagesCount = choiceImagesCount,
                    QuestionTitles = questionTitles,
                    HasActiveUserExams = hasActiveAttempts,
                    IsActive = exam.IsActive,
                    WarningMessage = warningMessage
                };
            },TimeSpan.FromMinutes(2));

            return data!;
        }
    }
}
