using FluentValidation;

namespace Exam_Online_API.Application.Features.Authentication.Verify2FA
{
    public class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
    {
        public Verify2FACommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Verification code is required.")
                .Length(6).WithMessage("Verification code must be 6 digits.");

            RuleFor(x => x.RememberMe)
                .NotNull().WithMessage("RememberMe flag must be specified.");
        }
    }
}
