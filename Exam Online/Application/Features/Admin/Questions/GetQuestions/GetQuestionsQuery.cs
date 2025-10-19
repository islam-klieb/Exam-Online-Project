using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.GetQuestions
{
    public class GetQuestionsQuery : IRequest<GetQuestionsResponse>
    {
        public int PageSize { get; set; } = 10;
        public DateTime? LastCreatedAt { get; set; }
        public string? SearchTerm { get; set; }
        public QuestionSortBy SortBy { get; set; } = QuestionSortBy.CreatedDate;
        public SortDirection SortDirection { get; set; } = SortDirection.Descending;
    }

    public class GetQuestionsResponse
    {
        public List<QuestionDto> Questions { get; set; } = new();
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public DateTime? NextCursor { get; set; }
    }

    public class QuestionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int ChoiceCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
