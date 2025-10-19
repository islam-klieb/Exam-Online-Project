using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Exams.DeleteExam
{
    public class DeleteExamCommand : IRequest<DeleteExamResponse>
    {
        public Guid Id { get; set; }
    }

    public class DeleteExamResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = "Exam deleted successfully";
        public int QuestionsDeleted { get; set; }
        public int ChoicesDeleted { get; set; }
        public int UserExamsDeleted { get; set; }
        public int UserAnswersDeleted { get; set; }
    }
}
