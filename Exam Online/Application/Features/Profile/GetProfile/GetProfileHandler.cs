using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.HybridCacheService;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.Profile.GetProfile
{
    public class GetProfileHandler : IRequestHandler<GetProfileQuery, GetProfileResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GetProfileHandler> _logger;
        private readonly IHybridCacheService _cache;

        public GetProfileHandler(
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            ILogger<GetProfileHandler> logger,
            IHybridCacheService cache)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _cache = cache;
        }

        public async Task<GetProfileResponse> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");

            var cacheKey = $"user:profile:{userId}";

            var cachedProfile = await _cache.GetAsync<GetProfileResponse>(cacheKey);
            if (cachedProfile != null)
            {
                _logger.LogInformation("Cache hit for user profile {UserId}", userId);
                return cachedProfile;
            }

            try
            {
                var user = await _userManager.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken);

                if (user == null)
                    throw new NotFoundException("User profile not found.");

                var response = new GetProfileResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    CreatedAt = user.CreatedAt
                };

                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15));

                _logger.LogInformation("User profile cached successfully for UserId: {UserId}", user.Id);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                throw new BusinessLogicException($"Failed to retrieve profile: {ex.Message}");
            }
        }
    }
}
