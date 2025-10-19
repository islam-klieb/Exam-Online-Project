using FluentValidation;

namespace Exam_Online_API.Application.Features.UserAnswers.GetUserAnswerHistory
{
    public class GetUserAnswerHistoryValidator : AbstractValidator<GetUserAnswerHistoryQuery>
    {
        public GetUserAnswerHistoryValidator()
        {
            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(50).WithMessage("Page size must not exceed 50");
        }
    }
}
