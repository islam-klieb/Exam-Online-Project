using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Domain.Entities
{
    public class Exam : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration {  get; set; }
        public ExamStatus Status { get; set; } = ExamStatus.Draft;
        public bool IsActive { get; set; }


        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = new Category();

        public List<Question> Questions { get; set; } = new List<Question>();
        public List<UserExam> UserExams { get; set; } = new List<UserExam>();

        
    }
}
