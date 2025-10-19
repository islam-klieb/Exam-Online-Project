using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Categories.DeleteCategory
{
    public class DeleteCategoryCommand : IRequest<DeleteCategoryResponse>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCategoryResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "Category deleted successfully";
    }
}
