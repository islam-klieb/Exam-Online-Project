using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.GetQuestionDeletionImpact
{
    public class GetQuestionDeletionImpactQuery : IRequest<QuestionDeletionImpact>
    {
        public Guid Id { get; set; }
    }

    public class QuestionDeletionImpact
    {
        public Guid QuestionId { get; set; }
        public string QuestionTitle { get; set; } = string.Empty;
        public string ExamTitle { get; set; } = string.Empty;
        public string CategoryTitle { get; set; } = string.Empty;
        public QuestionType QuestionType { get; set; }
        public int ChoiceCount { get; set; }
        public int UserAnswerCount { get; set; }
        public int UniqueUsersAffected { get; set; }
        public int ChoiceImagesCount { get; set; }
        public bool ExamIsActive { get; set; }
        public bool HasUserAnswers { get; set; }
        public string WarningMessage { get; set; } = string.Empty;
    }
}
