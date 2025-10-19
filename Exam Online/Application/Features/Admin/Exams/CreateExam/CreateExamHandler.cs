using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Factories;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Exams.CreateExam
{
    public class CreateExamHandler : IRequestHandler<CreateExamCommand, CreateExamResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<CreateExamHandler> _logger;
        private readonly ICacheInvalidationService _cache;
        public CreateExamHandler(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<CreateExamHandler> logger,
            ICacheInvalidationService cache)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<CreateExamResponse> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken);

                if (category == null)
                {
                    throw new NotFoundException($"Category with ID {request.CategoryId} not found");
                }

                var existingExam = await _context.Exams
                    .Where(e => e.CategoryId == request.CategoryId &&
                                e.Title.ToLower() == request.Title.Trim().ToLower())
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingExam != null)
                {
                    throw new ConflictException("An exam with this title already exists in the selected category");
                }

                var iconPath = await _fileService.UploadFileAsync(request.Icon,"exams",cancellationToken);

                var exam = ExamFactory.Create(
                               request.Title,
                               iconPath,
                               request.CategoryId,
                               request.StartDate,
                               request.EndDate,
                               request.Duration
                           );

                _context.Exams.Add(exam);
                await _context.SaveChangesAsync(cancellationToken);
                await _cache.InvalidateExamCacheAsync();

                _logger.LogInformation(
                    "Exam created successfully. ID: {ExamId}, Title: {Title}, Category: {CategoryId}, Status: {Status}",
                    exam.Id,
                    exam.Title,
                    exam.CategoryId,
                    exam.Status);

                return new CreateExamResponse
                {
                    Id = exam.Id,
                    Title = exam.Title,
                    IconPath = iconPath,
                    CategoryId = exam.CategoryId,
                    CategoryTitle = category.Title,
                    StartDate = exam.StartDate,
                    EndDate = exam.EndDate,
                    Duration = exam.Duration,
                    Status = exam.Status,
                    CreatedAt = exam.CreatedAt
                };
            }
            catch (NotFoundException)
            {
                throw;
            }
            catch (ConflictException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam with title: {Title}", request.Title);
                throw new BusinessLogicException($"Failed to create exam: {ex.Message}");
            }
        }
    }
}
