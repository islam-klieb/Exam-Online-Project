using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Features.ExamTaking.StartExam;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Shared.CalculateScore;
using Exam_Online_API.Shared.Submission;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.ExamTaking.ResumeExam
{
    public class ResumeExamHandler : IRequestHandler<ResumeExamQuery, ResumeExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ResumeExamHandler> _logger;

        public ResumeExamHandler(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ResumeExamHandler> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<ResumeExamResponse> Handle(ResumeExamQuery request, CancellationToken cancellationToken)
        {
            

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new BusinessLogicException("User not authenticated");

            var userExam = await _context.UserExams
                .Include(ue => ue.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .Include(ue => ue.userAnswers)
                .Where(ue => ue.ExamId == request.ExamId &&
                             ue.UserId == userId &&
                             ue.AttemptStatus == ExamAttemptStatus.InProgress)
                .OrderByDescending(ue => ue.AttemptDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (userExam == null)
            {
                return new ResumeExamResponse
                {
                    CanResume = false,
                    Message = "No active or in-progress exam found."
                };
            }

            var now = DateTime.UtcNow;
            var durationSeconds = userExam.Exam.Duration * 60;
            var elapsedSeconds = (int)(now - userExam.AttemptDate).TotalSeconds;
            var timeRemaining = durationSeconds - elapsedSeconds;

            if (timeRemaining <= 0)
            {
                _logger.LogWarning(
                    "Exam expired. Auto-submitting. UserExamId: {UserExamId}, Overdue: {Over}s",
                    userExam.Id, Math.Abs(timeRemaining));

                await AutoSubmitExpiredExam(userExam, cancellationToken);

                return new ResumeExamResponse
                {
                    CanResume = false,
                    Message = "Exam duration expired. Your exam was automatically submitted."
                };
            }

            userExam.LastActivityAt = now;
            await _context.SaveChangesAsync(cancellationToken);

            var savedAnswers = userExam.userAnswers
                .GroupBy(ua => ua.QuestionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(ua => ua.ChoiceId).ToList()
                );

            var expiryTime = userExam.AttemptDate.AddSeconds(durationSeconds);

            var questions = userExam.Exam.Questions.Select(q => new QuestionDto
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

            _logger.LogInformation(
                "Exam resumed successfully. UserExamId: {UserExamId}, Remaining: {Seconds}s, SavedAnswers: {AnswerCount}",
                userExam.Id, timeRemaining, savedAnswers.Count);

            return new ResumeExamResponse
            {
                CanResume = true,
                UserExamId = userExam.Id,
                ExamTitle = userExam.Exam.Title,
                TimeRemainingSeconds = timeRemaining,
                OriginalStartTime = userExam.AttemptDate,
                ExpiryTime = expiryTime,
                Questions = questions,
                SavedAnswers = savedAnswers,
                Message = $"Resuming exam — {Math.Ceiling(timeRemaining / 60.0)} minutes remaining."
            };
        }

        private async Task AutoSubmitExpiredExam(UserExam userExam, CancellationToken cancellationToken)
        {
            if (userExam.AttemptStatus != ExamAttemptStatus.InProgress)
            {
                _logger.LogWarning("Attempt to re-submit expired exam ignored. UserExamId: {UserExamId}", userExam.Id);
                return;
            }

            var now = DateTime.UtcNow;
            userExam.AttemptStatus = ExamAttemptStatus.TimedOut;
            userExam.DurationTaken = (int)(now - userExam.AttemptDate).TotalSeconds;
            userExam.LastActivityAt = now;

            var savedAnswers = userExam.userAnswers
                .GroupBy(ua => ua.QuestionId)
                .Select(g => new AnswerSubmission
                {
                    QuestionId = g.Key,
                    SelectedChoiceIds = g.Select(ua => ua.ChoiceId).ToList()
                })
                .ToList();

            var scoreResult = Score.CalculateScore(userExam.Exam.Questions, savedAnswers);
            userExam.Score = scoreResult.Score;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Auto-submitted expired exam. UserExamId: {UserExamId}, Final Score: {Score}",
                userExam.Id, scoreResult.Score);
        }
    }
}
