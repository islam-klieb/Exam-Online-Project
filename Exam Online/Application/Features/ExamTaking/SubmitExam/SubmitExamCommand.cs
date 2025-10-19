using Exam_Online_API.Domain.Enums;
using Exam_Online_API.Shared.Submission;
using MediatR;

namespace Exam_Online_API.Application.Features.ExamTaking.SubmitExam
{
    public class SubmitExamCommand : IRequest<SubmitExamResponse>
    {
        public Guid UserExamId { get; set; }
        public List<AnswerSubmission> Answers { get; set; } = new();
    }

    public class SubmitExamResponse
    {
        public Guid UserExamId { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public int CorrectAnswers { get; set; }
        public int IncorrectAnswers { get; set; }
        public int DurationTaken { get; set; } // in seconds
        public ExamAttemptStatus AttemptStatus { get; set; }
        public bool IsHighestScore { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
