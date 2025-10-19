namespace Exam_Online_API.Domain.Common
{
    public interface IAuditableEntity
    {
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string CreatedBy { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
