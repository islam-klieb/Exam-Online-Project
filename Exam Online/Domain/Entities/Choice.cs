using Exam_Online_API.Domain.Common;
using Exam_Online_API.Domain.Enums;
using Microsoft.VisualBasic.FileIO;

namespace Exam_Online_API.Domain.Entities
{
    public class Choice : BaseEntity
    {
        public string TextChoice { get; set; } = string.Empty;
        public bool IsCorrect { get; set;}

        public FileType? ChoiceType { get; set; }
        public string? ChoiceFilePath { get; set; }

        public Guid QuestionId { get; set; }
        public Question Question { get; set; } = new Question();

        public List<UserAnswer> userAnswers { get; set; } = new List<UserAnswer>();

    }
}
