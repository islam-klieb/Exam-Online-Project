using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Exams.ToggleExamStatus
{
    public class ToggleExamStatusCommand : IRequest<ToggleExamStatusResponse>
    {
        public Guid Id { get; set; }
    }

    public class ToggleExamStatusResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public ExamStatus Status { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
