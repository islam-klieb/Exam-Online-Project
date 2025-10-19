using Exam_Online_API.Application.Features.Admin.Questions.CreateQuestion;
using Exam_Online_API.Application.Features.Admin.Questions.DeleteQuestion;
using Exam_Online_API.Application.Features.Admin.Questions.GetQuestionDeletionImpact;
using Exam_Online_API.Application.Features.Admin.Questions.GetQuestions;
using Exam_Online_API.Application.Features.Admin.Questions.UpdateQuestion;
using Exam_Online_API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Questions
{
    [Route("api/admin/questions")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class QuestionsController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public QuestionsController(IMediator mediator, ILogger<QuestionsController> logger)
        {
            _mediator = mediator;
        }

        #region POST - Create Question
        [HttpPost]
        [ProducesResponseType(typeof(CreateQuestionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateQuestionResponse>> CreateQuestion([FromBody] CreateQuestionCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        #endregion

        #region PUT - Update Question
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UpdateQuestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateQuestionResponse>> UpdateQuestion(Guid id, [FromForm] UpdateQuestionCommand command)
        {
            command.Id = id;

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        #endregion

        #region DELETE - Delete Question
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(DeleteQuestionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteQuestionResponse>> DeleteQuestion(Guid id)
        {
            var response = await _mediator.Send(new DeleteQuestionCommand { Id = id });
            return Ok(response);
        }
        #endregion

        #region GET - Get Questions
        [HttpGet]
        [ProducesResponseType(typeof(GetQuestionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetQuestionsResponse>> GetQuestions(
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? lastCreatedAt = null,
            [FromQuery] string? searchTerm = null)
        {
            var query = new GetQuestionsQuery
            {
                PageSize = pageSize,
                LastCreatedAt = lastCreatedAt,
                SearchTerm = searchTerm
            };

            var response = await _mediator.Send(query);

            return Ok(response);
        }

        #endregion

        #region Delete Impact

        [HttpGet("{id}/deletion-impact")]
        [ProducesResponseType(typeof(QuestionDeletionImpact), StatusCodes.Status200OK)]
        public async Task<ActionResult<QuestionDeletionImpact>> GetQuestionDeletionImpact(Guid id)
        {
            var query = new GetQuestionDeletionImpactQuery { Id = id };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

    }
}
