using FluentValidation;

namespace Exam_Online_API.Application.Features.ExamTaking.StartExam
{
    public class StartExamValidator : AbstractValidator<StartExamCommand>
    {
        public StartExamValidator()
        {
            RuleFor(x => x.ExamId)
               .NotEmpty().WithMessage("Exam ID is required")
               .NotEqual(Guid.Empty).WithMessage("Invalid Exam ID");
        }
    }
}
