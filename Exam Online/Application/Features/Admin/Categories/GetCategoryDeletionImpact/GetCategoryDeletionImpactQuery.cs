using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Categories.GetCategoryDeletionImpact
{
    public class GetCategoryDeletionImpactQuery : IRequest<CategoryDeletionImpact>
    {
        public Guid Id { get; set; }
    }

    public class CategoryDeletionImpact
    {
        public Guid CategoryId { get; set; }
        public string CategoryTitle { get; set; } = string.Empty;
        public int ExamCount { get; set; }
        public int QuestionCount { get; set; }
        public int ChoiceCount { get; set; }
        public int UserExamCount { get; set; }
        public int UserAnswerCount { get; set; }
        public int UniqueUsersAffected { get; set; }
        public List<string> ExamTitles { get; set; } = new();
        public bool HasActiveExams { get; set; }
        public bool HasInProgressUserExams { get; set; }
        public string WarningMessage { get; set; } = string.Empty;
    }

}
