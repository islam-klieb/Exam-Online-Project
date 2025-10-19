using Exam_Online_API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Exams.GetExamsByCategory
{
    [Route("api/exams")]
    [Authorize]
    public class ExamsController : ApiBaseController
    {
        private readonly IMediator _mediator;
        public ExamsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("category/{categoryId:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(GetExamsByCategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetExamsByCategoryResponse>> GetExamsByCategory(
           Guid categoryId,
           [FromQuery] int pageNumber = 1,
           [FromQuery] int pageSize = 20)
        {
            var query = new GetExamsByCategoryQuery
            {
                CategoryId = categoryId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
