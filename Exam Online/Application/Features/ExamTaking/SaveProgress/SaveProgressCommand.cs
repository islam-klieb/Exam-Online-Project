using Exam_Online_API.Shared.Submission;
using MediatR;

namespace Exam_Online_API.Application.Features.ExamTaking.SaveProgress
{
    public class SaveProgressCommand : IRequest<SaveProgressResponse>
    {
        public Guid UserExamId { get; set; }
        public List<AnswerSubmission> Answers { get; set; } = new();
    }

    public class SaveProgressResponse
    {
        public bool IsSuccess { get; set; }
        public DateTime LastSaved { get; set; }
        public int AnswersSaved { get; set; }
    }
}
