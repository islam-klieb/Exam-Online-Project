using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Exam_Online_API.Application.Features.Admin.Exams.DeleteExam
{
    public class DeleteExamHandler : IRequestHandler<DeleteExamCommand, DeleteExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteExamHandler> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICacheInvalidationService _cache;


        public DeleteExamHandler(
            ApplicationDbContext context,
            ILogger<DeleteExamHandler> logger,
            ICacheInvalidationService cache,
            IBackgroundJobClient backgroundJobClient)
        {
            _context = context;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
        }

        public async Task<DeleteExamResponse> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {

            var exam = await _context.Exams
                .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices)
                .Include(e => e.UserExams)
                    .ThenInclude(ue => ue.userAnswers)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with ID {request.Id} not found");
            }

            try
            {
                var examIconPath = exam.Icon;

                var choiceImagePaths = exam.Questions
                    .SelectMany(q => q.Choices)
                    .Where(c => c.ChoiceType == Domain.Enums.FileType.Image && !string.IsNullOrEmpty(c.ChoiceFilePath))
                    .Select(c => c.ChoiceFilePath!)
                    .ToList();

                var questionCount = exam.Questions.Count;
                var choiceCount = exam.Questions.Sum(q => q.Choices.Count);
                var userExamCount = exam.UserExams.Count;
                var userAnswerCount = exam.UserExams.Sum(ue => ue.userAnswers.Count);

                _context.Exams.Remove(exam);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.InvalidateExamCacheAsync();

                var iconJobId = _backgroundJobClient.Enqueue<IFileService>(service => service.DeleteFileAsync(examIconPath, CancellationToken.None));

                _logger.LogInformation(
                    "Enqueued exam icon deletion job. JobId: {JobId}, File: {FilePath}",
                    iconJobId,
                    examIconPath);

                if (choiceImagePaths.Any())
                {
                    var batchJobId = _backgroundJobClient.Enqueue<IFileService>(service => service.DeleteFileAsync(choiceImagePaths, CancellationToken.None));

                    _logger.LogInformation(
                        "Enqueued batch choice image deletion job. JobId: {JobId}, ImageCount: {Count}",
                        batchJobId,
                        choiceImagePaths.Count);
                }

                _logger.LogInformation(
                    "Exam deleted successfully. " +
                    "ID: {ExamId}, Title: {Title}, " +
                    "Questions: {QuestionCount}, Choices: {ChoiceCount}, " +
                    "UserExams: {UserExamCount}, UserAnswers: {UserAnswerCount}, " +
                    "ChoiceImages: {ChoiceImageCount}",
                    exam.Id,
                    exam.Title,
                    questionCount,
                    choiceCount,
                    userExamCount,
                    userAnswerCount,
                    choiceImagePaths.Count);

                return new DeleteExamResponse
                {
                    IsSuccess = true,
                    Message = $"Exam '{exam.Title}' and all related data deleted successfully",
                    QuestionsDeleted = questionCount,
                    ChoicesDeleted = choiceCount,
                    UserExamsDeleted = userExamCount,
                    UserAnswersDeleted = userAnswerCount
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam with ID: {ExamId}", request.Id);
                throw new BusinessLogicException($"Failed to delete exam: {ex.Message}");
            }
        }
    }
}
