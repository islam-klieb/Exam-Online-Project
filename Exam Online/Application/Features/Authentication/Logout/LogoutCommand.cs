using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.Logout
{
    public class LogoutCommand : IRequest<bool> { }
}
