using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Exam_Online_API.Application.Features.Authentication.ResetPassword
{
    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly UserManager<User> _userManager;
        private readonly ILogger<ResetPasswordHandler> _logger;
        private readonly IValidator<ResetPasswordCommand> _validator;

        public ResetPasswordHandler(
            UserManager<User> userManager,
            ILogger<ResetPasswordHandler> logger,
            IValidator<ResetPasswordCommand> validator)
        {
            _userManager = userManager;
            _logger = logger;
            _validator = validator;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
           

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Password reset attempt for non-existent email: {Email}", request.Email);
                return false;
            }

            if (request.NewPassword != request.ConfirmPassword)
                throw new BusinessLogicException("Passwords do not match");

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to reset password for {Email}: {Error}", request.Email, errorMsg);
                throw new BusinessLogicException($"Failed to reset password: {errorMsg}");
            }

            _logger.LogInformation("Password reset successfully for {Email}", request.Email);
            return true;
        }
    }
}
