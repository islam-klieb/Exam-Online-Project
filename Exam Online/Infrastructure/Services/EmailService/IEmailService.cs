namespace Exam_Online_API.Infrastructure.Services.EmailService
{
    public interface IEmailService
    {
        Task SendPasswordResetEmailAsync(string email, string resetLink);
        Task<string> Send2FAAsync(string email, string password);
    }
}
