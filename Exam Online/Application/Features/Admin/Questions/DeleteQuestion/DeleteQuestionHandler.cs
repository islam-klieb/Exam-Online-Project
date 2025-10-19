using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Questions.DeleteQuestion
{
    public class DeleteQuestionHandler : IRequestHandler<DeleteQuestionCommand, DeleteQuestionResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DeleteQuestionHandler> _logger;
        private readonly ICacheInvalidationService _cache;

        public DeleteQuestionHandler(
            ApplicationDbContext context,
            IBackgroundJobClient backgroundJobClient,
            ILogger<DeleteQuestionHandler> logger,
            ICacheInvalidationService cache)
        {
            _context = context;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<DeleteQuestionResponse> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
        {
           
            try
            {
                var question = await _context.Questions
                    .Include(q => q.Choices)
                    .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

                if (question == null)
                {
                    _logger.LogWarning("Delete failed: Question {QuestionId} not found", request.Id);
                    throw new NotFoundException($"Question with ID {request.Id} not found");
                }

                var ChoiceFilePaths = question.Choices.Select(c=>c.ChoiceFilePath).ToList();
                
               _backgroundJobClient.Enqueue<IFileService>(service =>service.DeleteFileAsync(ChoiceFilePaths, CancellationToken.None));

               _logger.LogInformation("Enqueued background job to delete FilePaths for Choices in Question {QuestionId}", question.Id);

                int deletedChoices = question.Choices.Count;

                _context.Questions.Remove(question);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.InvalidateQuestionCacheAsync();

                _logger.LogInformation("Question {QuestionId} deleted successfully", request.Id);
                return new DeleteQuestionResponse
                {
                    IsSuccess = true,
                    QuestionId = question.Id,
                    DeletedChoiceCount = deletedChoices,
                    Message = $"Question '{question.Title}' and {deletedChoices} related choice(s) deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Question {QuestionId}", request.Id);
                throw new BusinessLogicException($"Failed to delete question: {ex.Message}");
            }
        }
    }
}
