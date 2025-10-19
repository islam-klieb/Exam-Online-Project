using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Exams.GetExamDeletionImpact
{
    public class GetExamDeletionImpactQuery : IRequest<ExamDeletionImpact>
    {
        public Guid Id { get; set; }
    }

    public class ExamDeletionImpact
    {
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public int QuestionCount { get; set; }
        public int ChoiceCount { get; set; }
        public int UserExamCount { get; set; }
        public int UserAnswerCount { get; set; }
        public int UniqueUsersAffected { get; set; }
        public int ChoiceImagesCount { get; set; }
        public List<string> QuestionTitles { get; set; } = new();
        public bool HasActiveUserExams { get; set; }
        public bool IsActive { get; set; }
        public string WarningMessage { get; set; } = string.Empty;
    }
}
