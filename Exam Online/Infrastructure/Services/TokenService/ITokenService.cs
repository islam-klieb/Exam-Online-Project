using Exam_Online_API.Domain.Entities;

namespace Exam_Online_API.Infrastructure.Services.TokenService
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateJwtToken(User user, bool rememberMe = false);
    }
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAccessToken { get; set; }
        public DateTime ExpiresRefreshToken { get; set; }
    }
}
