using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetExamReports
{
    [ApiController]
    [Route("api/admin/reports")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("exams")]
        [ProducesResponseType(typeof(GetExamReportsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetExamReportsResponse>> GetExamReports([FromQuery] GetExamReportsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
