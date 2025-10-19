using Exam_Online_API.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Exam_Online_API.Domain.Entities
{
    public class User : IdentityUser , IAuditableEntity
    {

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public string? GoogleId { get; set; }
        public string? ProfilePicture { get; set; }

        public bool IsTwoFactorEnabled { get; set; }
        public string? TwoFactorSecretKey { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }




        public List<UserExam> UserExams { get; set; } = new List<UserExam>();

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt {  get; set; } = DateTime.UtcNow;
        public string CreatedBy {  get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set;} 
        public DateTime? DeletedAt { get; set;}
        public string? DeletedBy { get; set;}
    }
}
