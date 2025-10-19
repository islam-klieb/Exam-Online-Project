using Exam_Online_API.Application.Common.Exceptions;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.CacheInvalidationService;
using Exam_Online_API.Infrastructure.Services.FileService;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Exam_Online_API.Application.Features.Profile.UpdateProfile
{
    public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFileService _fileService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<UpdateProfileHandler> _logger;
        private readonly ICacheInvalidationService _cache;

        public UpdateProfileHandler(
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            IFileService fileService,
            IBackgroundJobClient backgroundJobClient,
            ILogger<UpdateProfileHandler> logger,
            ICacheInvalidationService cache)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _fileService = fileService;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
            _cache = cache;
        }

        public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    throw new UnauthorizedAccessException("User not authenticated.");

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id.ToString() == userId, cancellationToken);

                if (user == null)
                    throw new NotFoundException("User profile not found.");

                if (request.ProfileImage != null)
                {
                    if (!string.IsNullOrEmpty(user.ProfilePicture))
                    {
                        _backgroundJobClient.Enqueue<IFileService>(svc =>svc.DeleteFileAsync(user.ProfilePicture, CancellationToken.None));
                    }

                    var uploaded = await _fileService.UploadFileAsync(request.ProfileImage, "profiles", cancellationToken);
                    user.ProfilePicture = uploaded;
                    _logger.LogInformation("Profile image updated for user {UserId}", user.Id);
                }

                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

                await _userManager.UpdateAsync(user);

                await _cache.InvalidateUserCacheAsync();

                _logger.LogInformation("Profile updated successfully for user {UserId}", user.Id);

                return new UpdateProfileResponse
                {
                    IsSuccess = true,
                    Message = "Profile updated successfully.",
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    ProfilePicture = user.ProfilePicture
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                throw new BusinessLogicException($"Failed to update profile: {ex.Message}");
            }
        }
    }
}
