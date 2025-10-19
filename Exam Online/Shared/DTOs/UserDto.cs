namespace Exam_Online_API.Shared.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class AuthResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresTokenAt { get; set; }
        public UserDto User { get; set; } = new();
        public bool RequiresTwoFactor { get; set; }
        public string? Message { get; set; }
    }
}
