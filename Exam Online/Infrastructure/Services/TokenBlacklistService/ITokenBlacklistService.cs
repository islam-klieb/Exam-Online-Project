namespace Exam_Online_API.Infrastructure.Services.TokenBlacklistService
{
    public interface ITokenBlacklistService
    {
        Task AddToBlacklistAsync(string token, TimeSpan? expiry = null);
        Task<bool> IsTokenBlacklistedAsync(string token);
    }
}
