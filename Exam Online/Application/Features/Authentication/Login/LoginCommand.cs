using Exam_Online_API.Shared.DTOs;
using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.Login
{
    public class LoginCommand : IRequest<AuthResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
