using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Categories.DeleteCategory
{
    public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid category ID");
        }
    }
}
