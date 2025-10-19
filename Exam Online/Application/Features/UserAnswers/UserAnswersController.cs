using Exam_Online_API.Application.Features.UserAnswers.GetExamAttemptDetails;
using Exam_Online_API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.UserAnswers.GetUserAnswerHistory
{
    [Route("api/user-answers")]
    [Authorize] 
    public class UserAnswersController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public UserAnswersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("details/{userExamId:guid}")]
        [ProducesResponseType(typeof(ExamAttemptDetailsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ExamAttemptDetailsResponse>> GetAttemptDetails(Guid userExamId)
        {
            var query = new GetExamAttemptDetailsQuery { UserExamId = userExamId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("history")]
        [ProducesResponseType(typeof(GetUserAnswerHistoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetUserAnswerHistoryResponse>> GetHistory(
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? examId = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] DateTime? lastAttemptDate = null)
        {
            var query = new GetUserAnswerHistoryQuery
            {
                PageSize = pageSize,
                ExamId = examId,
                CategoryId = categoryId,
                LastAttemptDate = lastAttemptDate
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
