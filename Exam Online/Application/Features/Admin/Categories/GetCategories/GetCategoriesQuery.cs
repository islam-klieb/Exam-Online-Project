using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Categories.GetCategories
{
    public class GetCategoriesQuery : IRequest<GetCategoriesResponse>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public CategorySortBy SortBy { get; set; } = CategorySortBy.Name;
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }

    public class GetCategoriesResponse
    {
        public List<CategoryDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class CategoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ExamCount { get; set; }
    }
}
