using FluentValidation;

namespace Exam_Online_API.Application.Features.UserAnswers.GetExamAttemptDetails
{
    public class GetExamAttemptDetailsValidator : AbstractValidator<GetExamAttemptDetailsQuery>
    {
        public GetExamAttemptDetailsValidator()
        {
            RuleFor(ux => ux.UserExamId)
             .NotEmpty().WithMessage("UserExam ID is required");
        }
    }
}
