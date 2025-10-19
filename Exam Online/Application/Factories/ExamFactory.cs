using Exam_Online_API.Domain.Entities;
using Exam_Online_API.Domain.Enums;

namespace Exam_Online_API.Application.Factories
{
    public static class ExamFactory
    {
        public static Exam Create(
            string title,
            string iconPath,
            Guid categoryId,
            DateTime startDate,
            DateTime endDate,
            int duration,
            ExamStatus status = ExamStatus.Scheduled,
            bool isActive = true)
        {
            var exam = new Exam
            {
                Title = title.Trim(),
                Icon = iconPath ?? string.Empty,
                CategoryId = categoryId,
                StartDate = startDate,
                EndDate = endDate,
                Duration = duration,
                Status = status,
                IsActive = isActive
            };

            return exam;
        }
    }
}
