using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Factories;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Exam_Online_API.Application.Features.Admin.Categories.CreateCategory
{
    public class CreateCategoryHandler : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileService _fileService;
        private readonly ILogger<CreateCategoryHandler> _logger;
        private readonly ICacheInvalidationService _cacheInvalidation;

        public CreateCategoryHandler(
            ApplicationDbContext context,
            IFileService fileService,
            ILogger<CreateCategoryHandler> logger,
            ICacheInvalidationService cacheInvalidation)
        {
            _context = context;
            _fileService = fileService;
            _logger = logger;
            _cacheInvalidation = cacheInvalidation;
        }

        public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var existingCategory = await _context.Categories
                                            .AsNoTracking()
                                            .Where(c => c.Title.ToLower() == request.Title.ToLower())
                                            .FirstOrDefaultAsync(cancellationToken);

                if (existingCategory is not null)
                {
                    throw new ConflictException("A category with this title already exists");
                }

                var iconPath = await _fileService.UploadFileAsync(request.Icon,"categories",cancellationToken);

                var category = CategoryFactory.Create(request.Title, iconPath);

                _context.Categories.Add(category);

                await _context.SaveChangesAsync(cancellationToken);

                await _cacheInvalidation.InvalidateCategoryCacheAsync();

                _logger.LogInformation("Category created successfully with ID: {CategoryId}", category.Id);

                return new CreateCategoryResponse
                {
                    Id = category.Id,
                    Title = category.Title,
                    IconPath = iconPath,
                    CreatedAt = category.CreatedAt
                };

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "Error creating category with title: {Title}", request.Title);
                throw;
            }

        }
    }
}
