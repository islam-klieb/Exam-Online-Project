namespace Exam_Online_API.Domain.Common
{
    public abstract class BaseEntity : IAuditableEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
