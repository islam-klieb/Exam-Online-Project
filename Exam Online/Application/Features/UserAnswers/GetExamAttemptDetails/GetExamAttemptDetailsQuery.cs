using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.UserAnswers.GetExamAttemptDetails
{
    public class GetExamAttemptDetailsQuery : IRequest<ExamAttemptDetailsResponse>
    {
        public Guid UserExamId { get; set; }
    }

    public class ExamAttemptDetailsResponse
    {
        public Guid UserExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptDate { get; set; }
        public int DurationTaken { get; set; }
        public ExamAttemptStatus AttemptStatus { get; set; }
        public List<QuestionAnswerDto> QuestionsAndAnswers { get; set; } = new();
    }

    public class QuestionAnswerDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public bool IsCorrect { get; set; }
        public List<ChoiceAnswerDto> Choices { get; set; } = new();
    }

    public class ChoiceAnswerDto
    {
        public Guid ChoiceId { get; set; }
        public string TextChoice { get; set; } = string.Empty;
        public FileType? ChoiceType { get; set; }
        public string? ChoiceFilePath { get; set; }
        public bool IsCorrect { get; set; }
        public bool WasSelected { get; set; }
    }
}
