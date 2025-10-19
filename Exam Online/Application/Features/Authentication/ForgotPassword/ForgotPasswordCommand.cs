using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.ForgotPassword
{
    public class ForgotPasswordCommand : IRequest<bool>
    {
        public string Email { get; set; } = string.Empty;
    }
    public class BaseUrl
    {
        public string Url { get; set; } = string.Empty;
    }
}
