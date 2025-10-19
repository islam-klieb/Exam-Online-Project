namespace Exam_Online_API.Shared.Submission
{
    public class AnswerSubmission
    {
        public Guid QuestionId { get; set; }
        public List<Guid> SelectedChoiceIds { get; set; } = new();
    }
}
