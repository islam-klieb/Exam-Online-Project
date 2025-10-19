using MediatR;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Application.Features.UserAnswers.GetUserAnswerHistory
{
    public class GetUserAnswerHistoryQuery : IRequest<GetUserAnswerHistoryResponse>
    {
        public int PageSize { get; set; } = 10;

        public Guid? ExamId { get; set; }
        public Guid? CategoryId { get; set; }

        public DateTime? LastAttemptDate { get; set; }
    }

    public class GetUserAnswerHistoryResponse
    {
        public List<UserExamHistoryDto> History { get; set; } = new();
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public DateTime? NextCursor { get; set; }
    }

    public class UserExamHistoryDto
    {
        public Guid UserExamId { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string ExamIcon { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public int Score { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime AttemptDate { get; set; }
        public int DurationTaken { get; set; }
        public ExamAttemptStatus AttemptStatus { get; set; }
        public bool IsHighestScore { get; set; }
    }
}
