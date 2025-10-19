using Exam_Online.Infrastructure.Persistence;
using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Infrastructure.Services.EmailService;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;

namespace Exam_Online_API.Application.Features.Authentication.ForgotPassword
{
    public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private readonly BaseUrl _baseUrl;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ForgotPasswordHandler> _logger;

        public ForgotPasswordHandler(UserManager<User> userManager, 
                                    IEmailService emailService,
                                    IOptions<BaseUrl> baseUrl,
                                    ApplicationDbContext context,
                                    ILogger<ForgotPasswordHandler> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _baseUrl = baseUrl.Value;
            _context = context;
            _logger = logger;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = $"{_baseUrl}/reset-password?email={user.Email}&token={encodedToken}";
            await _emailService.SendPasswordResetEmailAsync(user.Email!, resetLink);


            _logger.LogInformation("Password reset link generated for {Email}", user.Email);

            return true;
        }
    }
}
