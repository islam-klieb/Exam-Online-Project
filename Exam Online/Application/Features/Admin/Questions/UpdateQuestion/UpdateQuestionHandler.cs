using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Questions.UpdateQuestion
{


   public class UpdateQuestionHandler : IRequestHandler<UpdateQuestionCommand, UpdateQuestionResponse>
   {
       private readonly ApplicationDbContext _context;
       private readonly IFileService _fileService;
       private readonly ILogger<UpdateQuestionHandler> _logger;
       private readonly IBackgroundJobClient _backgroundJobClient;
       private readonly ICacheInvalidationService _cache;

       public UpdateQuestionHandler(
           ApplicationDbContext context,
           IFileService fileService,
           ILogger<UpdateQuestionHandler> logger,
           IBackgroundJobClient backgroundJobClient,
           ICacheInvalidationService cache)
       {
           _context = context;
           _fileService = fileService;
           _logger = logger;
           _backgroundJobClient = backgroundJobClient;
            _cache = cache;
       }

       public async Task<UpdateQuestionResponse> Handle(UpdateQuestionCommand request, CancellationToken cancellationToken)
       {
            var question = await _context.Questions
               .Include(q => q.Choices)
               .FirstOrDefaultAsync(q => q.Id == request.Id, cancellationToken);

           if (question == null)
               throw new NotFoundException($"Question with ID {request.Id} not found");

           try
           {
               question.Title = request.Title.Trim();
               question.Type = request.Type;

               foreach (var choiceDto in request.Choices)
               {
                   if (choiceDto.IsDeleted && choiceDto.Id.HasValue)
                   {
                       var choiceToRemove = question.Choices.FirstOrDefault(c => c.Id == choiceDto.Id.Value);
                       if (choiceToRemove != null)
                       {
                           if (!string.IsNullOrEmpty(choiceToRemove.ChoiceFilePath))
                           {
                               _backgroundJobClient.Enqueue<IFileService>(service => 
                                            service.DeleteFileAsync(choiceToRemove.ChoiceFilePath, CancellationToken.None));
                           }
                           _context.Choices.Remove(choiceToRemove);
                       }
                       continue;
                   }

                   if (choiceDto.Id.HasValue)
                   {
                       var choice = question.Choices.First(c => c.Id == choiceDto.Id.Value);
                       choice.TextChoice = choiceDto.Text ?? string.Empty;
                       choice.IsCorrect = choiceDto.IsCorrect;

                       if (choiceDto.File != null)
                       {
                           var oldPath = choice.ChoiceFilePath;

                           choice.ChoiceFilePath = await _fileService.UploadFileAsync(choiceDto.File, "choices", cancellationToken);
                           choice.ChoiceType = FileType.Image;

                           if (!string.IsNullOrEmpty(oldPath))
                           {
                               _backgroundJobClient.Enqueue<IFileService>(service =>service.DeleteFileAsync(oldPath, CancellationToken.None));
                           }
                       }
                   }
                   else
                   {
                       string? filePath = null;
                       FileType? fileType = null;

                       if (choiceDto.File != null)
                       {
                           filePath = await _fileService.UploadFileAsync(choiceDto.File, "choices", cancellationToken);
                           fileType = FileType.Image;
                       }
                       else if (!string.IsNullOrWhiteSpace(choiceDto.Text))
                       {
                           fileType = FileType.Text;
                       }

                       question.Choices.Add(new Choice
                       {
                           TextChoice = choiceDto.Text ?? string.Empty,
                           IsCorrect = choiceDto.IsCorrect,
                           ChoiceFilePath = filePath,
                           ChoiceType = fileType
                       });
                   }
               }

               await _context.SaveChangesAsync(cancellationToken);

               await _cache.InvalidateQuestionCacheAsync();

               _logger.LogInformation("Question updated successfully. ID: {QuestionId}", question.Id);

               return new UpdateQuestionResponse
               {
                   Id = question.Id,
                   Title = question.Title,
                   Type = question.Type,
                   ChoiceCount = question.Choices.Count,
                   UpdatedAt = DateTime.UtcNow
               };
           }
           catch (Exception ex)
           {
               _logger.LogError(ex, "Error updating question with ID: {QuestionId}", request.Id);
               throw new BusinessLogicException($"Failed to update question: {ex.Message}");
           }
       }
   }
}


