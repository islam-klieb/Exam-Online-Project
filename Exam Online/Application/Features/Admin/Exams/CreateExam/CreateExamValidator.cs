using Exam_Online_API.Shared.Validators;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Exams.CreateExam
{
    public class CreateExamValidator : AbstractValidator<CreateExamCommand>
    {
        public CreateExamValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(3, 20).WithMessage("Title must be between 3 and 20 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Title must contain only letters and spaces");

            RuleFor(x => x.Icon)
                .NotNull().WithMessage("Icon image is required")
                .Must(ImageValidationHelper.BeValidImageFile).WithMessage("Invalid image file. Only JPG, JPEG, and PNG are allowed")
                .Must(icon => ImageValidationHelper.BeValidFileSize(icon, 2)).WithMessage("File size must be less than 2MB");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("Category is required")
                .NotEqual(Guid.Empty).WithMessage("Please select a valid category");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .Must(BeFutureDate).WithMessage("Start date must be after current date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be equal to or after start date");

            RuleFor(x => x.Duration)
                .GreaterThanOrEqualTo(20).WithMessage("Duration must be at least 20 minutes")
                .LessThanOrEqualTo(180).WithMessage("Duration must not exceed 180 minutes (3 hours)");
        }
        private bool BeFutureDate(DateTime date)
        {
            return date > DateTime.UtcNow;
        }
    }
}
