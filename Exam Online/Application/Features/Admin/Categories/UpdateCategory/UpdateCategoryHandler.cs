using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryHandler : IRequestHandler<UpdateCategoryCommand, UpdateCategoryResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<UpdateCategoryHandler> _logger;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ICacheInvalidationService _cache;

        public UpdateCategoryHandler(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<UpdateCategoryHandler> logger,
            IValidator<UpdateCategoryCommand> validator,
            IBackgroundJobClient backgroundJobClient,
            ICacheInvalidationService cache)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _backgroundJobClient = backgroundJobClient;
            _cache = cache;
        }

        public async Task<UpdateCategoryResponse> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {request.Id} not found");
            }

            try
            {
                if (category.Title.ToLower() != request.Title.Trim().ToLower())
                {
                    var titleExists = await _context.Categories
                                     .AnyAsync(c => c.Id != request.Id && c.Title.ToLower() == request.Title.Trim().ToLower(),cancellationToken);

                    if (titleExists)
                    {
                         throw new ConflictException("A category with this title already exists");
                    }
                }

                var oldIconPath = category.Icon;

                category.Title = request.Title.Trim();

                if (request.Icon != null)
                {
                    var newIconPath = await _fileService.UploadFileAsync(request.Icon,"categories",cancellationToken);

                    category.Icon = newIconPath;

                    var jobId = _backgroundJobClient.Enqueue<IFileService>(service => service.DeleteFileAsync(oldIconPath, CancellationToken.None));

                    _logger.LogInformation("Enqueued file deletion job. JobId: {JobId}, File: {FilePath}, category: {categoryId}",
                        jobId, oldIconPath, category.Id);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await _cache.InvalidateCategoryCacheAsync();

                _logger.LogInformation(
                    "Category updated successfully. ID: {CategoryId}, New Title: {Title}, Icon Changed: {IconChanged}",
                    category.Id,category.Title,request.Icon != null);

                return new UpdateCategoryResponse
                {
                    Id = category.Id,
                    Title = category.Title,
                    IconPath = category.Icon,
                    UpdatedAt = category.UpdatedAt ?? DateTime.UtcNow
                };
            }
            catch (ConflictException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category with ID: {CategoryId}", request.Id);
                throw new BusinessLogicException($"Failed to update category: {ex.Message}");
            }
        }
    }
}
