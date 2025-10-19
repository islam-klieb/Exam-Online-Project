using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;

namespace Exam_Online_API.Application.Features.Admin.Categories.DeleteCategory
{
    public class DeleteCategoryHandler : IRequestHandler<DeleteCategoryCommand, DeleteCategoryResponse>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DeleteCategoryHandler> _logger;
        private readonly ICacheInvalidationService _cacheInvalidation;
       

        public DeleteCategoryHandler(
            ApplicationDbContext context,
            ILogger<DeleteCategoryHandler> logger,
            ICacheInvalidationService cacheInvalidation)
        {
            _context = context;
            _logger = logger;
            _cacheInvalidation = cacheInvalidation;
        }

        public async Task<DeleteCategoryResponse> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _context.Categories
                .Include(c => c.Exams)
                .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

            if (category == null)
            {
                throw new NotFoundException($"Category with ID {request.Id} not found");
            }

            try
            {
                _context.Categories.Remove(category);

                await _context.SaveChangesAsync(cancellationToken);

                await _cacheInvalidation.InvalidateCategoryCacheAsync();

                _logger.LogInformation(
                    "Category deleted successfully. ID: {CategoryId}, Title: {Title}, Related Exams: {ExamCount}",
                    category.Id,
                    category.Title,
                    category.Exams.Count);

                return new DeleteCategoryResponse
                {
                    IsSuccess = true,
                    Message = $"Category '{category.Title}' and {category.Exams.Count} related exam(s) deleted successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category with ID: {CategoryId}", request.Id);
                throw new BusinessLogicException($"Failed to delete category: {ex.Message}");
            }
        }
    }
}
