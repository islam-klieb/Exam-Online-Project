using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.TokenService;
using Exam_Online_API.Shared.DTOs;
using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Exam_Online_API.Application.Features.Authentication.GoogleLogin
{
    public class GoogleLoginHandler : IRequestHandler<GoogleLoginCommand, AuthResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _tokenService;

        public GoogleLoginHandler(UserManager<User> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken);
            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new User
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    EmailConfirmed = true,
                    GoogleId = payload.Subject,
                    ProfilePicture = payload.Picture
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddToRoleAsync(user, "User");
            }

            var token = await _tokenService.GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return new AuthResponse
            {
                Token = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresTokenAt = token.ExpiresAccessToken,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    UserName = user.UserName!,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfilePicture = user.ProfilePicture,
                    Roles = roles.ToList()
                }
            };
        }
    }
}
