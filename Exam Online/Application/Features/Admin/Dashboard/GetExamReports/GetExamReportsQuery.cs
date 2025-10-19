using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetExamReports
{
    public class GetExamReportsQuery : IRequest<GetExamReportsResponse>
    {
        public string? SearchTerm { get; set; }
        public ExamSortBy SortBy { get; set; } = ExamSortBy.CreatedDate;
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetExamReportsResponse
    {
        public List<ExamReportDto> Exams { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ExamReportDto
    {
        public Guid ExamId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public ExamStatus Status { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
