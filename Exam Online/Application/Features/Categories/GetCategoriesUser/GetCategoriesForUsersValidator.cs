using FluentValidation;

namespace Exam_Online_API.Application.Features.Categories.GetCategoriesUser
{
    public class GetCategoriesForUsersValidator : AbstractValidator<GetCategoriesForUsersQuery>
    {
        public GetCategoriesForUsersValidator() 
        {
            RuleFor(c => c.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(c => c.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
        }
    }
}
