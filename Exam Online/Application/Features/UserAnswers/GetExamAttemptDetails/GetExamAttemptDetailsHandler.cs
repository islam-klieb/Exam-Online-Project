using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.UserAnswers.GetExamAttemptDetails
{
    public class GetExamAttemptDetailsHandler : IRequestHandler<GetExamAttemptDetailsQuery, ExamAttemptDetailsResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetExamAttemptDetailsHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetExamAttemptDetailsHandler(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetExamAttemptDetailsHandler> logger,
            IHybridCacheService cache)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _cache = cache;
        }

        public async Task<ExamAttemptDetailsResponse> Handle(GetExamAttemptDetailsQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new BusinessLogicException("User not authenticated.");

            var cacheKey = $"user:{userId}:exam-attempt:{request.UserExamId}";

            var cachedResponse = await _cache.GetAsync<ExamAttemptDetailsResponse>(cacheKey);
            if (cachedResponse != null)
            {
                _logger.LogInformation("Cache hit for ExamAttempt {UserExamId}", request.UserExamId);
                return cachedResponse;
            }

            try
            {
                var userExam = await _context.UserExams
                    .AsNoTracking()
                    .Include(ue => ue.Exam)
                        .ThenInclude(e => e.Category)
                    .Include(ue => ue.Exam)
                        .ThenInclude(e => e.Questions)
                            .ThenInclude(q => q.Choices)
                    .Include(ue => ue.userAnswers)
                        .ThenInclude(ua => ua.Choice)
                    .FirstOrDefaultAsync(ue =>
                        ue.Id == request.UserExamId && ue.UserId == userId,
                        cancellationToken);

                if (userExam is null)
                    throw new NotFoundException($"Exam attempt with ID {request.UserExamId} not found.");

                var questionsAndAnswers = new List<QuestionAnswerDto>();
                var correctCount = 0;

                foreach (var question in userExam.Exam.Questions)
                {
                    var selectedIds = userExam.userAnswers
                        .Where(ua => ua.QuestionId == question.Id)
                        .Select(ua => ua.ChoiceId)
                        .OrderBy(id => id)
                        .ToList();

                    var correctIds = question.Choices
                        .Where(c => c.IsCorrect)
                        .Select(c => c.Id)
                        .OrderBy(id => id)
                        .ToList();

                    var isCorrect = selectedIds.SequenceEqual(correctIds);
                    if (isCorrect) correctCount++;

                    var choiceDtos = question.Choices.Select(c => new ChoiceAnswerDto
                    {
                        ChoiceId = c.Id,
                        TextChoice = c.TextChoice,
                        ChoiceType = c.ChoiceType,
                        ChoiceFilePath = c.ChoiceFilePath,
                        IsCorrect = c.IsCorrect,
                        WasSelected = selectedIds.Contains(c.Id)
                    }).ToList();

                    questionsAndAnswers.Add(new QuestionAnswerDto
                    {
                        QuestionId = question.Id,
                        QuestionTitle = question.Title,
                        QuestionType = question.Type,
                        IsCorrect = isCorrect,
                        Choices = choiceDtos
                    });
                }

                var response = new ExamAttemptDetailsResponse
                {
                    UserExamId = userExam.Id,
                    ExamTitle = userExam.Exam.Title,
                    CategoryTitle = userExam.Exam.Category.Title,
                    Score = userExam.Score,
                    CorrectAnswers = correctCount,
                    TotalQuestions = userExam.Exam.Questions.Count,
                    AttemptDate = userExam.AttemptDate,
                    DurationTaken = userExam.DurationTaken,
                    AttemptStatus = userExam.AttemptStatus,
                    QuestionsAndAnswers = questionsAndAnswers
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

                _logger.LogInformation("Cached exam attempt details for UserExamId: {UserExamId}", userExam.Id);

                return response;
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (FluentValidation.ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam attempt details for {UserExamId}", request.UserExamId);
                throw new BusinessLogicException($"Failed to retrieve exam attempt details: {ex.Message}");
            }
        }
    }
}
