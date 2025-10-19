using Exam_Online_API.Shared.DTOs;
using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.GoogleLogin
{
    public class GoogleLoginCommand : IRequest<AuthResponse>
    {
        public string IdToken { get; set; } = string.Empty;
    }
}
