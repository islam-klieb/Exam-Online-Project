using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.ExamTaking.StartExam
{
    public class StartExamHandler : IRequestHandler<StartExamCommand, StartExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StartExamHandler> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StartExamHandler(
            ApplicationDbContext context,
            ILogger<StartExamHandler> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<StartExamResponse> Handle(StartExamCommand request, CancellationToken cancellationToken)
        {

            

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new BusinessLogicException("User not authenticated");
            }

            var exam = await _context.Exams
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with ID {request.ExamId} not found");
            }

            var now = DateTime.UtcNow;

            if (!exam.IsActive || exam.Status != ExamStatus.Active)
            {
                throw new BusinessLogicException("This exam is not currently active");
            }

            if (exam.StartDate > now)
            {
                throw new BusinessLogicException($"This exam has not started yet. It will be available on {exam.StartDate:yyyy-MM-dd HH:mm}");
            }

            if (exam.EndDate < now)
            {
                throw new BusinessLogicException("This exam has already ended");
            }

            if (!exam.Questions.Any())
            {
                throw new BusinessLogicException("This exam has no questions");
            }

            try
            {
                var userExam = new UserExam
                {
                    UserId = userId,
                    ExamId = exam.Id,
                    AttemptDate = now,
                    DurationTaken = 0,
                    Score = 0,
                    AttemptStatus = ExamAttemptStatus.InProgress,
                    LastActivityAt = now
                };

                _context.UserExams.Add(userExam);
                await _context.SaveChangesAsync(cancellationToken);

                var expiryTime = now.AddMinutes(exam.Duration);

                _logger.LogInformation(
                    "User started exam. UserId: {UserId}, ExamId: {ExamId}, UserExamId: {UserExamId}, ExpiryTime: {ExpiryTime}",
                    userId,
                    exam.Id,
                    userExam.Id,
                    expiryTime);

                var questions = exam.Questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Title = q.Title,
                    Type = q.Type,
                    Choices = q.Choices.Select(c => new ChoiceDto
                    {
                        Id = c.Id,
                        TextChoice = c.TextChoice,
                        ChoiceType = c.ChoiceType,
                        ChoiceFilePath = c.ChoiceFilePath
                    }).ToList()
                }).ToList();

                return new StartExamResponse
                {
                    UserExamId = userExam.Id,
                    ExamId = exam.Id,
                    ExamTitle = exam.Title,
                    Duration = exam.Duration,
                    StartTime = now,
                    ExpiryTime = expiryTime,
                    Questions = questions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting exam for user: {UserId}, exam: {ExamId}", userId, request.ExamId);
                throw new BusinessLogicException($"Failed to start exam: {ex.Message}");
            }
        }
    }
}
