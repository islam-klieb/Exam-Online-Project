using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Application.Features.Authentication.Register;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.TokenService;
using Exam_Online_API.Shared.DTOs;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Exam_Online_API.Application.Features.Authentication.Verify2FA
{
    public class Verify2FAHandler : IRequestHandler<Verify2FACommand, AuthResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IValidator<Verify2FACommand> _validator;
        public Verify2FAHandler(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            ITokenService tokenService,
            IValidator<Verify2FACommand> validator)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _validator = validator;
        }

        public async Task<AuthResponse> Handle(Verify2FACommand request, CancellationToken cancellationToken)
        {
           
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new BusinessLogicException("User not found.");

            var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                user,
                TokenOptions.DefaultEmailProvider,
                request.Code);

            if (!isValid)
                throw new BusinessLogicException("Invalid or expired verification code.");

            var token = await _tokenService.GenerateJwtToken(user, request.RememberMe);
            var roles = await _userManager.GetRolesAsync(user);

            user.RefreshToken = token.RefreshToken;
            user.RefreshTokenExpiryTime = token.ExpiresRefreshToken;

            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresTokenAt = token.ExpiresAccessToken,
                RequiresTwoFactor = false,
                Message = "Login successful.",
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                }
            };
        }
    }
}
