using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Exams.ToggleExamStatus
{
    public class ToggleExamStatusValidator : AbstractValidator<ToggleExamStatusCommand>
    {
        public ToggleExamStatusValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exam ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid exam ID");
        }
    }
}
