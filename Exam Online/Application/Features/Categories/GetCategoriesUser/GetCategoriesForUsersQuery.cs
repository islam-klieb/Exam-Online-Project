using MediatR;

namespace Exam_Online_API.Application.Features.Categories.GetCategoriesUser
{
    public class GetCategoriesForUsersQuery : IRequest<GetCategoriesForUsersResponse>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class GetCategoriesForUsersResponse
    {
        public List<CategoryForUserDto> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }

    public class CategoryForUserDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ActiveExamCount { get; set; }
    }
}
