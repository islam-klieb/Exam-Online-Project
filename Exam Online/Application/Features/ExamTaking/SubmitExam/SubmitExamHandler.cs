using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Shared.CalculateScore;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.ExamTaking.SubmitExam
{
    public class SubmitExamHandler : IRequestHandler<SubmitExamCommand, SubmitExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SubmitExamHandler> _logger;

        public SubmitExamHandler(
            ApplicationDbContext context,
            ILogger<SubmitExamHandler> logger,
            IValidator<SubmitExamCommand> validator)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<SubmitExamResponse> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
        {
            

            var userExam = await _context.UserExams
                .Include(ue => ue.Exam)
                    .ThenInclude(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                .FirstOrDefaultAsync(ue => ue.Id == request.UserExamId, cancellationToken);

            if (userExam == null)
            {
                throw new NotFoundException($"User exam attempt with ID {request.UserExamId} not found");
            }

            if (userExam.AttemptStatus != ExamAttemptStatus.InProgress)
            {
                throw new BusinessLogicException("This exam attempt has already been completed");
            }

            try
            {
                var now = DateTime.UtcNow;
                var durationTaken = (int)(now - userExam.AttemptDate).TotalSeconds;
                var examDurationInSeconds = userExam.Exam.Duration * 60;

                var isTimedOut = durationTaken > examDurationInSeconds;
                var attemptStatus = isTimedOut ? ExamAttemptStatus.TimedOut : ExamAttemptStatus.Completed;

                var scoreResult = Score.CalculateScore(userExam.Exam.Questions, request.Answers);

                foreach (var answer in request.Answers)
                {
                    foreach (var choiceId in answer.SelectedChoiceIds)
                    {
                        var userAnswer = new UserAnswer
                        {
                            UserExamId = userExam.Id,
                            QuestionId = answer.QuestionId,
                            ChoiceId = choiceId
                        };
                        _context.UserAnswers.Add(userAnswer);
                    }
                }

                userExam.Score = scoreResult.Score;
                userExam.DurationTaken = durationTaken;
                userExam.AttemptStatus = attemptStatus;
                userExam.LastActivityAt = now;

                await _context.SaveChangesAsync(cancellationToken);

                var isHighestScore = await IsHighestScore(
                    userExam.UserId,
                    userExam.ExamId,
                    userExam.Score,
                    cancellationToken);

                _logger.LogInformation(
                    "Exam submitted. UserExamId: {UserExamId}, Score: {Score}/{TotalQuestions}, " + "Status: {Status}, IsHighestScore: {IsHighestScore}",
                     userExam.Id,scoreResult.CorrectAnswers,scoreResult.TotalQuestions,attemptStatus,isHighestScore);


                var message = isTimedOut ? "Time expired! Your exam was automatically submitted." : isHighestScore
                                                                                                  ? "Congratulations! This is your highest score for this exam!"
                                                                                                  : "Exam submitted successfully!";
                return new SubmitExamResponse
                {
                    UserExamId = userExam.Id,
                    Score = scoreResult.Score,
                    TotalQuestions = scoreResult.TotalQuestions,
                    CorrectAnswers = scoreResult.CorrectAnswers,
                    IncorrectAnswers = scoreResult.IncorrectAnswers,
                    DurationTaken = durationTaken,
                    AttemptStatus = attemptStatus,
                    IsHighestScore = isHighestScore,
                    Message = message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting exam: {UserExamId}", request.UserExamId);
                throw new BusinessLogicException($"Failed to submit exam: {ex.Message}");
            }
        }
        private async Task<bool> IsHighestScore(string userId,Guid examId,int currentScore,CancellationToken cancellationToken)
        {
            var maxScore = await _context.UserExams
                .Where(ue => ue.UserId == userId &&
                             ue.ExamId == examId &&
                             ue.AttemptStatus == ExamAttemptStatus.Completed)
                .MaxAsync(ue => (int?)ue.Score, cancellationToken);

            return maxScore == null || currentScore >= maxScore;
        }

        private class ScoreResult
        {
            public int Score { get; set; }
            public int CorrectAnswers { get; set; }
            public int IncorrectAnswers { get; set; }
            public int TotalQuestions { get; set; }
        }
    }
}
