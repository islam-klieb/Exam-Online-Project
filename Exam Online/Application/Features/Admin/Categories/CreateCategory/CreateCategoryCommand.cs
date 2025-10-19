using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Categories.CreateCategory
{
    public class CreateCategoryCommand : IRequest<CreateCategoryResponse>
    {
        public string Title { get; set; } = string.Empty;
        public IFormFile Icon { get; set; } = null!;
    }

    public class CreateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = "Category created successfully";
    }
}
