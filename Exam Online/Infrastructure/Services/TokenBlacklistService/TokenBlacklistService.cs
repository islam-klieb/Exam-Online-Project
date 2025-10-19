using StackExchange.Redis;

namespace Exam_Online_API.Infrastructure.Services.TokenBlacklistService
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDatabase _db;
        private const string BlacklistPrefix = "blacklist:token:";

        public TokenBlacklistService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task AddToBlacklistAsync(string token, TimeSpan? expiry = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                return;

            await _db.StringSetAsync(BlacklistPrefix + token, "true", expiry ?? TimeSpan.FromHours(24));
        }

        public async Task<bool> IsTokenBlacklistedAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            return await _db.KeyExistsAsync(BlacklistPrefix + token);
        }
    }
}
