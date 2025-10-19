using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Exams.CreateExam
{
    public class CreateExamCommand : IRequest<CreateExamResponse>
    {
        public string Title { get; set; } = string.Empty;
        public IFormFile Icon { get; set; } = null!;
        public Guid CategoryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
    }
    public class CreateExamResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string IconPath { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public ExamStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Message { get; set; } = "Exam created successfully";
    }
}
