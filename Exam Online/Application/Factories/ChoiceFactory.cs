using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Application.Factories
{
    public static class ChoiceFactory
    {
        public static Choice Create(string TextChoice , bool IsCorrect , string ChoiceFilePath ,FileType? ChoiceType)
        {
            var choice = new Choice
            {
                TextChoice = TextChoice.Trim() ,
                IsCorrect = IsCorrect ,
                ChoiceFilePath = ChoiceFilePath ,
                ChoiceType = ChoiceType
            };
            return choice;
        }
    }
}
