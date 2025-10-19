using Exam_Online_API.Shared.Validators;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Exams.UpdateExam
{
    public class UpdateExamValidator : AbstractValidator<UpdateExamCommand>
    {
        public UpdateExamValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Exam ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid exam ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(3, 20).WithMessage("Title must be between 3 and 20 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Title must contain only letters and spaces");

            RuleFor(x => x.Icon)
                .Must(ImageValidationHelper.BeValidImageFile).WithMessage("Invalid image file. Only JPG, PNG, and GIF are allowed")
                .Must(icon=>ImageValidationHelper.BeValidFileSize(icon,2)).WithMessage("File size must be less than 2MB")
                .When(x => x.Icon != null);

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required")
                .NotEqual(Guid.Empty).WithMessage("Please select a valid category");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be equal to or after start date");

            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(20).WithMessage("Duration must be at least 20 minutes")
                .LessThanOrEqualTo(180).WithMessage("Duration must not exceed 180 minutes (3 hours)");
        }
    }
}
