using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryResponse>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public IFormFile? Icon { get; set; }
    }

    public class UpdateCategoryResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string Message { get; set; } = "Category updated successfully";
    }
}
