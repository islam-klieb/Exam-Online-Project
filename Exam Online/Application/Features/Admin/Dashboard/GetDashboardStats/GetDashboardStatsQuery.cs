using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetDashboardStats
{
    public class GetDashboardStatsQuery : IRequest<GetDashboardStatsResponse> { }

    public class GetDashboardStatsResponse
    {
        public int TotalUsers { get; set; }
        public int TotalCategories { get; set; }
        public int TotalExams { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalAttempts { get; set; }
        public int ActiveExams { get; set; }
        public int CompletedExams { get; set; }
        public double? AverageScore { get; set; }
    }
}
