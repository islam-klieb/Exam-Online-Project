using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Questions.GetQuestions
{
    public class GetQuestionsValidator : AbstractValidator<GetQuestionsQuery>
    {
        public GetQuestionsValidator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(50)
                .WithMessage("Search term must not exceed 50 characters.");
        }
    }
}
