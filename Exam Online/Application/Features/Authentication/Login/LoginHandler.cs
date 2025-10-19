using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.EmailService;
using Exam_Online_API.Infrastructure.Services.TokenService;
using Exam_Online_API.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Exam_Online_API.Application.Features.Authentication.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, AuthResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;

        public LoginHandler(UserManager<User> userManager, 
                            SignInManager<User> signInManager,
                            ITokenService tokenService,
                            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailService = emailService;
        }

        public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) throw new BusinessLogicException("Invalid credentials");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);
            if (!result.Succeeded) throw new BusinessLogicException("Invalid credentials");

            if (user.IsTwoFactorEnabled)
            {
                var Token = await _userManager.GenerateTwoFactorTokenAsync(user,TokenOptions.DefaultEmailProvider);
                var Message = await _emailService.Send2FAAsync(user.Email!, Token);

                return new AuthResponse
                {
                    RequiresTwoFactor = true,
                    Message = Message,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email!,
                        UserName = user.UserName!
                    }
                };


            }

            var jwt = await _tokenService.GenerateJwtToken(user, request.RememberMe);
            var roles = await _userManager.GetRolesAsync(user);
            user.RefreshToken = jwt.RefreshToken;
            user.RefreshTokenExpiryTime = jwt.ExpiresRefreshToken;

            await _userManager.UpdateAsync(user);

            return new AuthResponse
            {
                Token = jwt.AccessToken,
                RefreshToken = jwt.RefreshToken,
                ExpiresTokenAt = jwt.ExpiresAccessToken,
                RequiresTwoFactor = false,
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
