using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.ExamTaking.StartExam
{
    public class StartExamCommand : IRequest<StartExamResponse>
    {
        public Guid ExamId { get; set; }
    }

    public class StartExamResponse
    {
        public Guid UserExamId { get; set; }
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime ExpiryTime { get; set; }
        public List<QuestionDto> Questions { get; set; } = new();
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<ChoiceDto> Choices { get; set; } = new();
    }

    public class ChoiceDto
    {
        public Guid Id { get; set; }
        public string TextChoice { get; set; } = string.Empty;
        public FileType? ChoiceType { get; set; }
        public string? ChoiceFilePath { get; set; }
    }
}
