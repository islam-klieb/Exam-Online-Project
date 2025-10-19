using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetCategoryAnalytics
{
    public class GetCategoryAnalyticsQuery : IRequest<GetCategoryAnalyticsResponse>
    {
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetCategoryAnalyticsResponse
    {
        public List<CategoryAnalyticsDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class CategoryAnalyticsDto
    {
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int ExamCount { get; set; }
        public int QuestionCount { get; set; }
        public int AttemptCount { get; set; }
        public double? AverageScore { get; set; }
    }
}
