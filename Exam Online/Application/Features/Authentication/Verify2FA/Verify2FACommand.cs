using Exam_Online_API.Shared.DTOs;
using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.Verify2FA
{
    public class Verify2FACommand : IRequest<AuthResponse>
    {
        public string Email { get; set; } = default!;
        public string Code { get; set; } = default!;
        public bool RememberMe { get; set; } = false;
    }
}
