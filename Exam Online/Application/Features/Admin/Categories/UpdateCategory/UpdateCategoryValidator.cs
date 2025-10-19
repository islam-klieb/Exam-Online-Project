using Exam_Online_API.Shared.Validators;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Admin.Categories.UpdateCategory
{
    public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Category ID is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid category ID");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .Length(3, 20).WithMessage("Title must be between 3 and 20 characters")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("Title must contain only letters and spaces");

            RuleFor(x => x.Icon)
                .Must(ImageValidationHelper.BeValidImageFile).WithMessage("Invalid image file. Only JPG, JPEG, and PNG are allowed")
                .Must(icon => ImageValidationHelper.BeValidFileSize(icon,2)).WithMessage("File size must be less than 2MB")
                .When(x => x.Icon != null); 
        }
    }
}
