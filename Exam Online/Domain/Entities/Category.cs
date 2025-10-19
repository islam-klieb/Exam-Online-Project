
using Exam_Online_API.Domain.Common;

namespace Exam_Online_API.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        public List<Exam> Exams { get; set; } = new List<Exam>();

    }
}
