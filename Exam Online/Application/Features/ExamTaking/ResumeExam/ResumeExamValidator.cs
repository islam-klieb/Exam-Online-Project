using FluentValidation;

namespace Exam_Online_API.Application.Features.ExamTaking.ResumeExam
{
    public class ResumeExamValidator : AbstractValidator<ResumeExamQuery>
    {
        public ResumeExamValidator()
        {
            RuleFor(x => x.ExamId)
              .NotEmpty().WithMessage("UserExam ID is required");
        }
    }
}
