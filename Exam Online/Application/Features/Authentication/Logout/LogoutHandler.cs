using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.TokenBlacklistService;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.Authentication.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, bool>
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LogoutHandler> _logger;
        private readonly ITokenBlacklistService _blacklistService;


        public LogoutHandler(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<LogoutHandler> logger,
            ITokenBlacklistService blacklistService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _blacklistService = blacklistService;
        }

        public async Task<bool> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Logout attempted without authentication.");
                throw new BusinessLogicException("User not authenticated.");
            }

            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
                            .ToString()
                            .Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

                if (!string.IsNullOrEmpty(token))
                {
                    await _blacklistService.AddToBlacklistAsync(token, TimeSpan.FromHours(24));
                }

                var user = await _userManager.FindByIdAsync(userId);
                user!.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                
                await _userManager.UpdateAsync(user);

                await _signInManager.SignOutAsync();

                _logger.LogInformation("User {UserId} logged out successfully.", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging out user {UserId}", userId);
                throw new BusinessLogicException("Failed to log out user.");
            }
        }
    }
}
