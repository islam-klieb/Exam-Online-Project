using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.DeleteQuestion
{
    public class DeleteQuestionCommand : IRequest<DeleteQuestionResponse>
    {
        //DeleteQuestionValidator
        public Guid Id { get; set; }
    }
    public class DeleteQuestionResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid QuestionId { get; set; }
        public int DeletedChoiceCount { get; set; }
    }
}
