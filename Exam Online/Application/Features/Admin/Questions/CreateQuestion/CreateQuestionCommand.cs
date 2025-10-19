using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.CreateQuestion
{
    public class CreateQuestionCommand : IRequest<CreateQuestionResponse>
    {
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public Guid ExamId { get; set; }
        public List<CreateChoiceDto> Choices { get; set; } = new();
    }

    public class CreateChoiceDto
    {
        public string? Text { get; set; }
        public IFormFile? File { get; set; }
        public bool IsCorrect { get; set; }
    }

    public class CreateQuestionResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public Guid ExamId { get; set; }
        public int ChoiceCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
