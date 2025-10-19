using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.TokenService;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Exam_Online_API.Application.Features.Authentication.RefreshToken
{
    public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly ITokenService _token;
        private readonly ILogger<RefreshTokenHandler> _logger;

        public RefreshTokenHandler(UserManager<User> userManager, ITokenService token, ILogger<RefreshTokenHandler> logger)
        {
            _userManager = userManager;
            _token = token;
            _logger = logger;
        }

        public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.RefreshToken == request.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            var Token = await _token.GenerateJwtToken(user);
            var newAccessToken = Token.AccessToken;
            var newRefreshToken = Token.RefreshToken;

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = Token.ExpiresRefreshToken;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Refreshed token for user {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAccessTokenAt = Token.ExpiresAccessToken
            };
        }
    }
}
