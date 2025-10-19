using FluentValidation;

namespace Exam_Online_API.Application.Features.Authentication.Register
{
    public class RegisterValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).Matches(@"^[a-zA-Z0-9_]+$");
            RuleFor(x => x.FirstName).NotEmpty().MinimumLength(3).Matches(@"^[a-zA-Z]+$");
            RuleFor(x => x.LastName).NotEmpty().MinimumLength(3).Matches(@"^[a-zA-Z]+$");
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Must contain an uppercase letter")
                .Matches("[a-z]").WithMessage("Must contain a lowercase letter")
                .Matches("[0-9]").WithMessage("Must contain a number")
                .Matches("[^a-zA-Z0-9]").WithMessage("Must contain a special character");
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password);
        }
    }
}
