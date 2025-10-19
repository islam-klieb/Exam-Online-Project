using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Categories.GetCategories
{
    public class GetCategoriesValidator : AbstractValidator<GetCategoriesQuery>
    {
        public GetCategoriesValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(50).WithMessage("Search term must not exceed 50 characters");
        }
    }
}
