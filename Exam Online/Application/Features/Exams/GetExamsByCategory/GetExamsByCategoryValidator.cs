using Exam_Online_API.Application.Features.Categories.GetCategoriesUser;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Exams.GetExamsByCategory
{
    public class GetExamsByCategoryValidator : AbstractValidator<GetExamsByCategoryQuery>
    {
        public GetExamsByCategoryValidator() 
        {
            RuleFor(e => e.CategoryId)
                .NotEmpty().WithMessage("Category ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid category ID");
        }
    }
}
