using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Exam_Online_API.Infrastructure.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly MailSettings _mailSettings;

        public EmailService(ILogger<EmailService> logger, IOptions<MailSettings> mailSettings)
        {
            _logger = logger;
            _mailSettings = mailSettings.Value;
        }
        public async Task<string> Send2FAAsync(string email, string code)
        {
            try
            {
                var emailBody = Build2FAEmailBody(email, code);

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.DisplayName ?? "Security Team", _mailSettings.Email));
                message.To.Add(MailboxAddress.Parse(email)); 
                message.Subject = "Your 2FA Verification Code";

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = emailBody
                };

                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("2FA email sent successfully to {Email}", email);
                return "2FA verification code sent successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending 2FA email to {Email}", email);
                return "Failed to send 2FA verification email. Please try again.";
            }
        }

 
        private static string Build2FAEmailBody(string email, string code)
        {
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; color: #333; margin: 20px; }");
            sb.AppendLine("h1 { color: #007bff; }");
            sb.AppendLine("p { margin: 10px 0; }");
            sb.AppendLine(".code { font-size: 24px; font-weight: bold; color: #28a745; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine($"<p>Dear {System.Net.WebUtility.HtmlEncode(email)},</p>");
            sb.AppendLine("<p>Thank you for using our application!</p>");
            sb.AppendLine("<p>To complete your login process, please use the following verification code:</p>");
            sb.AppendLine($"<p class='code'>{System.Net.WebUtility.HtmlEncode(code)}</p>");
            sb.AppendLine("<p>This code is valid for a short period, so please use it promptly.</p>");
            sb.AppendLine("<p>If you did not request this code, please ignore this email.</p>");
            sb.AppendLine("<p>Best regards,</p>");
            sb.AppendLine("<p>The Security Team</p>");

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            try
            {
                var html = new StringBuilder();
                html.AppendLine("<html><body>");
                html.AppendLine($"<p>Dear {System.Net.WebUtility.HtmlEncode(email)},</p>");
                html.AppendLine("<p>We received a request to reset your password.</p>");
                html.AppendLine($"<p><a href='{resetLink}' style='background-color:#007bff;color:#fff;padding:10px 15px;text-decoration:none;border-radius:5px;'>Reset Password</a></p>");
                html.AppendLine("<p>If you didn’t request this, please ignore this email.</p>");
                html.AppendLine("<p>Best regards,<br/>The Security Team</p>");
                html.AppendLine("</body></html>");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_mailSettings.DisplayName ?? "Security Team", _mailSettings.Email));
                message.To.Add(MailboxAddress.Parse(email));
                message.Subject = "Password Reset Request";

                message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = html.ToString() };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_mailSettings.Email, _mailSettings.Password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            }
        }
    }
}
