using Exam_Online_API.Domain.Common;

namespace Exam_Online_API.Domain.Entities
{
    public class UserAnswer : BaseEntity
    {


        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = new Question();

        public Guid ChoiceId { get; set; } 
        public Choice Choice { get; set; } = new Choice();

        public Guid UserExamId { get; set; } 
        public UserExam UserExam { get; set; } = new UserExam();

    }
}
