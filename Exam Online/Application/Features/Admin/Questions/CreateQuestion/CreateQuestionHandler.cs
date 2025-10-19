using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Factories;
using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.CreateQuestion
{
    public class CreateQuestionHandler : IRequestHandler<CreateQuestionCommand, CreateQuestionResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<CreateQuestionHandler> _logger;
        private readonly ICacheInvalidationService _cache;

        public CreateQuestionHandler(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<CreateQuestionHandler> logger,
            ICacheInvalidationService cache)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<CreateQuestionResponse> Handle(CreateQuestionCommand request, CancellationToken cancellationToken)
        {
            

            var exam = await _context.Exams.FindAsync(request.ExamId);

            if (exam is null)
                throw new NotFoundException("Exam not found");

            var question =  QuestionFactory.Create(request.Title,request.Type,request.ExamId);

            foreach (var choiceDto in request.Choices)
            {
                string? filePath = null;
                FileType? fileType = null;

                if (choiceDto.File is not null)
                {
                    filePath = await _fileService.UploadFileAsync(choiceDto.File, "choices", cancellationToken);
                    fileType = FileType.Image;
                }
                else if (!string.IsNullOrWhiteSpace(choiceDto.Text))
                {
                    fileType = FileType.Text;
                }

                var choice = ChoiceFactory.Create(choiceDto.Text!,
                                                  choiceDto.IsCorrect,
                                                  filePath!,
                                                  fileType);
                question.Choices.Add(choice);
            }

            _context.Questions.Add(question);
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.InvalidateQuestionCacheAsync();

            _logger.LogInformation("Question {QuestionId} created for Exam {ExamId}", question.Id, request.ExamId);

            return new CreateQuestionResponse
            {
                Id = question.Id,
                Title = question.Title,
                Type = question.Type,
                ExamId = question.ExamId,
                ChoiceCount = question.Choices.Count,
                CreatedAt = question.CreatedAt
            };
        }
    }

}
