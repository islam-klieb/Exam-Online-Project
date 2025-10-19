using FluentValidation;

namespace Exam_Online_API.Application.Features.ExamTaking.SubmitExam
{
    public class SubmitExamValidator : AbstractValidator<SubmitExamCommand>
    {
        public SubmitExamValidator()
        {
            RuleFor(x => x.UserExamId)
                .NotEmpty().WithMessage("UserExam ID is required");

            RuleFor(x => x.Answers)
                .NotEmpty().WithMessage("Answers are required");

            RuleForEach(x => x.Answers).ChildRules(answer =>
            {
                answer.RuleFor(a => a.QuestionId)
                    .NotEmpty().WithMessage("Question ID is required");

                answer.RuleFor(a => a.SelectedChoiceIds)
                    .NotEmpty().WithMessage("At least one choice must be selected for each question");
            });
        }
    }
}
