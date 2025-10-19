using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Exams.DeleteExam
{
    public class DeleteExamValidator : AbstractValidator<DeleteExamCommand>
    {
        public DeleteExamValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exam ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid exam ID");
        }
    }
}
