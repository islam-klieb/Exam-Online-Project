using Exam_Online_API.Application.Features.ExamTaking.StartExam;
using MediatR;

namespace Exam_Online_API.Application.Features.ExamTaking.ResumeExam
{
    public class ResumeExamQuery : IRequest<ResumeExamResponse>
    {
        public Guid ExamId { get; set; }
    }

    public class ResumeExamResponse
    {
        public bool CanResume { get; set; }
        public Guid? UserExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int TimeRemainingSeconds { get; set; }
        public DateTime OriginalStartTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
        public Dictionary<Guid, List<Guid>> SavedAnswers { get; set; } = new(); // QuestionId -> ChoiceIds
        public string Message { get; set; } = string.Empty;
    }
}
