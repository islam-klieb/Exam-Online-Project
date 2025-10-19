using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Domain.Entities
{
    public class UserExam : BaseEntity
    {
        public int Score { get; set; }
        public DateTime AttemptDate { get; set; }
        public int DurationTaken { get; set; }
        public ExamAttemptStatus AttemptStatus { get; set; } = ExamAttemptStatus.InProgress;
        public DateTime? LastActivityAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = new User();

        public Guid ExamId { get; set; }
        public Exam Exam { get; set; } = new Exam();

        public List<UserAnswer> userAnswers { get; set; } = new List<UserAnswer>();

       
    }
}
