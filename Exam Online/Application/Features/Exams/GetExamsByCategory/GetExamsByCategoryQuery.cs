using MediatR;

namespace Exam_Online_API.Application.Features.Exams.GetExamsByCategory
{
    public class GetExamsByCategoryQuery : IRequest<GetExamsByCategoryResponse>
    {
        public Guid CategoryId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20; 
    }

    public class GetExamsByCategoryResponse
    {
        public List<ExamForUserDto> Exams { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
    }

    public class ExamForUserDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public int QuestionCount { get; set; }
    }
}
