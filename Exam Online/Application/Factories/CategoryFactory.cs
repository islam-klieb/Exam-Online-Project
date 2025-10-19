using Exam_Online_API.Domain.Entities;

namespace Exam_Online_API.Application.Factories
{
    public static class CategoryFactory
    {
        public static Category Create(string title, string iconPath)
        {
            return new Category
            {
                Title = title.Trim(),
                Icon = iconPath ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
