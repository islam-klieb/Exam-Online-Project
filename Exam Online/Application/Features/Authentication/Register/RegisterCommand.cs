using MediatR;

namespace Exam_Online_API.Application.Features.Authentication.Register
{
    public class RegisterCommand : IRequest<RegisterResponse>
    {
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public bool IsTwoFactorEnabled {  get; set; } = false;
    }

    public class RegisterResponse
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = "Registration successful";
    }
}
