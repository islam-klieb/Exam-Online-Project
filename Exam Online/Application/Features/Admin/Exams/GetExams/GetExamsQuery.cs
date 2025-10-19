using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Exams.GetExams
{
    public class GetExamsQuery : IRequest<GetExamsResponse>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public DateTime? StartDateFrom { get; set; }
        public DateTime? StartDateTo { get; set; }
        public DateTime? EndDateFrom { get; set; }
        public DateTime? EndDateTo { get; set; }
        public int? DurationMin { get; set; }
        public int? DurationMax { get; set; }
        public ExamSortBy SortBy { get; set; } = ExamSortBy.Title;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    public class GetExamsResponse
    {
        public List<ExamDto> Exams { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class ExamDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public ExamStatus Status { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
    }
}
