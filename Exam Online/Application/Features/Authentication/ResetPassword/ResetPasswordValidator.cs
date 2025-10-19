using FluentValidation;

namespace Exam_Online_API.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().EmailAddress();

            RuleFor(x => x.Token)
                .NotEmpty();

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Must contain an uppercase letter")
                .Matches("[a-z]").WithMessage("Must contain a lowercase letter")
                .Matches("[0-9]").WithMessage("Must contain a number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match");
        }
    }
}
