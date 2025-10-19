using Exam_Online_API.Infrastructure.Services.TokenBlacklistService;

namespace Exam_Online_API.Middlewares.TokenBlacklistMiddleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenBlacklistService _blacklistService;

        public TokenBlacklistMiddleware(RequestDelegate next, ITokenBlacklistService blacklistService)
        {
            _next = next;
            _blacklistService = blacklistService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Replace("Bearer ", "", StringComparison.OrdinalIgnoreCase);

                if (await _blacklistService.IsTokenBlacklistedAsync(token))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Token has been revoked. Please log in again.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
