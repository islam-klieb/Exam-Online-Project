using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Questions.DeleteQuestion
{
    public class DeleteQuestionValidator : AbstractValidator<DeleteQuestionCommand>
    {
        public DeleteQuestionValidator()
        {
            RuleFor(q => q.Id).NotEmpty().WithMessage("Question ID is required");
        }
    }
}
