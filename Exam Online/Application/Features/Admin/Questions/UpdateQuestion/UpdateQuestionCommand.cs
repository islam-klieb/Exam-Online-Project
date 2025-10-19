using Exam_Online_API.Domain.Enums;
using MediatR;

namespace Exam_Online_API.Application.Features.Admin.Questions.UpdateQuestion
{
    public class UpdateQuestionCommand : IRequest<UpdateQuestionResponse>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public List<UpdateChoiceDto> Choices { get; set; } = new();
    }

    public class UpdateChoiceDto
    {
        public Guid? Id { get; set; }   
        public string? Text { get; set; }
        public IFormFile? File { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsDeleted { get; set; } 
    }

    public class UpdateQuestionResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public int ChoiceCount { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
