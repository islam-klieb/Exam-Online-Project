namespace Exam_Online_API.Infrastructure.Services.EmailService
{
    public class MailSettings
    {
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty ;
        public string Password { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
    }
}
