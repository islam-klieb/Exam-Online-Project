using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Domain.Entities
{
    public class Question : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }  = QuestionType.SingleChoice;


        public Guid ExamId { get; set; } 
        public Exam Exam { get; set; } = new Exam();

        public List<Choice> Choices { get; set; } = new List<Choice>();
        public List<UserAnswer> userAnswers { get; set; } = new List<UserAnswer>();

    }
}
