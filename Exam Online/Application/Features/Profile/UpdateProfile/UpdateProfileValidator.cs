using Exam_Online_API.Shared.Validators;
using FluentValidation;

namespace Exam_Online_API.Application.Features.Profile.UpdateProfile
{
    public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
    {
        public UpdateProfileValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required.")
                .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.")
                .Matches("^[a-zA-Z]+$").WithMessage("First name must contain only letters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.")
                .Matches("^[a-zA-Z]+$").WithMessage("Last name must contain only letters.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?\d{7,15}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("Invalid phone number format.");

            When(x => x.ProfileImage != null, () =>
            {
                RuleFor(x => x.ProfileImage)
                    .Must(ImageValidationHelper.BeValidImageFile)
                    .WithMessage("Profile image must be a JPG or PNG file.");

                RuleFor(x => x.ProfileImage)
                    .Must(ProfileImage => ImageValidationHelper.BeValidFileSize(ProfileImage, 2))
                    .WithMessage("Profile image must not exceed 2MB.");
            });
        }
    }
}
