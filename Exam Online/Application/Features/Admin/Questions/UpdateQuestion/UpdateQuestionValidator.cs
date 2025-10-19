using Exam_Online_API.Shared.Validators;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Questions.UpdateQuestion
{
    public class UpdateQuestionValidator : AbstractValidator<UpdateQuestionCommand>
    {
        public UpdateQuestionValidator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Question ID is required");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(3, 100).WithMessage("Title must be between 3 and 100 characters");

            RuleFor(x => x.Choices)
                .NotEmpty().WithMessage("At least one choice is required")
                .Must(c => c.Any(ch => ch.IsCorrect))
                .WithMessage("At least one choice must be marked correct");

            RuleForEach(x => x.Choices).ChildRules(choice =>
            {
                choice.RuleFor(c => c)
                    .Must(c => c.Text is null && c.File is null)
                    .WithMessage("Each choice must have text or image file.");

                choice.RuleFor(c => c.File)
                      .Must(ImageValidationHelper.BeValidImageFile).WithMessage("Invalid image file. Only JPG, PNG, and GIF are allowed")
                      .Must(file => ImageValidationHelper.BeValidFileSize(file, 2)).WithMessage("File size must be less than 2MB")
                      .When(c => c.File != null);

                choice.RuleFor(c => c.Text)
                      .Length(3, 200).WithMessage("Title must be between 3 and 200 characters")
                      .When(c => c.Text != null);
            });
        }
    }

}
