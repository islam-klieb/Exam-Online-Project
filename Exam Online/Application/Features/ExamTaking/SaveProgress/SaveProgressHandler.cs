using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.ExamTaking.SaveProgress
{
    public class SaveProgressHandler : IRequestHandler<SaveProgressCommand, SaveProgressResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SaveProgressHandler> _logger;

        public SaveProgressHandler(
            ApplicationDbContext context,
            ILogger<SaveProgressHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SaveProgressResponse> Handle(SaveProgressCommand request, CancellationToken cancellationToken)
        {
            

            var userExam = await _context.UserExams
                .Include(ue => ue.userAnswers)
                .AsTracking()
                .FirstOrDefaultAsync(ue => ue.Id == request.UserExamId, cancellationToken);

            if (userExam is null)
                throw new NotFoundException($"User exam attempt with ID {request.UserExamId} not found");

            if (userExam.AttemptStatus != ExamAttemptStatus.InProgress)
                throw new BusinessLogicException("Cannot save progress for a completed or expired exam");

            try
            {
                var questionIds = request.Answers.Select(a => a.QuestionId).ToList();
                var existingAnswers = userExam.userAnswers
                    .Where(ua => questionIds.Contains(ua.QuestionId))
                    .ToList();

                if (existingAnswers.Any())
                    _context.UserAnswers.RemoveRange(existingAnswers);

                var newAnswers = request.Answers
                    .SelectMany(a => a.SelectedChoiceIds.Select(choiceId => new UserAnswer
                    {
                        UserExamId = userExam.Id,
                        QuestionId = a.QuestionId,
                        ChoiceId = choiceId
                    }))
                    .ToList();

                await _context.UserAnswers.AddRangeAsync(newAnswers, cancellationToken);

                userExam.LastActivityAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Progress saved successfully | UserExamId: {UserExamId} | Questions: {QuestionCount} | Timestamp: {Time}",
                    userExam.Id, request.Answers.Count, userExam.LastActivityAt);

                return new SaveProgressResponse
                {
                    IsSuccess = true,
                    LastSaved = userExam.LastActivityAt.Value,
                    AnswersSaved = request.Answers.Count
                };
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error while saving progress for UserExamId: {UserExamId}", request.UserExamId);
                throw new BusinessLogicException("Database error occurred while saving progress. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving progress for UserExamId: {UserExamId}", request.UserExamId);
                throw new BusinessLogicException($"Failed to save progress: {ex.Message}");
            }
        }
    }
}
