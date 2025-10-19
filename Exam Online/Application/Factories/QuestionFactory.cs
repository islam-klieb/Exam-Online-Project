using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Application.Factories
{
    public static class QuestionFactory
    {
        
        public static Question Create(string Title, QuestionType Type, Guid ExamId)
        {
            var question = new Question
            {
                Title = Title.Trim() ,
                Type = Type ,
                ExamId = ExamId
            };
            return question;
        }
    }
}
