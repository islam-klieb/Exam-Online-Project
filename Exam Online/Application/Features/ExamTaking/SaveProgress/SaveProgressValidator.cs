using FluentValidation;

namespace Exam_Online_API.Application.Features.ExamTaking.SaveProgress
{
    public class SaveProgressValidator : AbstractValidator<SaveProgressCommand>
    {
        public SaveProgressValidator()
        {
            RuleFor(x => x.UserExamId)
                .NotEmpty().WithMessage("UserExamId is required.");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("Answers cannot be empty.");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .NotEmpty().WithMessage("QuestionId is required.");

                answer.RuleFor(a => a.SelectedChoiceIds)
                    .NotEmpty().WithMessage("At least one choice must be selected.");
            });
        }
    }
}
