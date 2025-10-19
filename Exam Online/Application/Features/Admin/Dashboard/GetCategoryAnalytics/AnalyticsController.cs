using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetCategoryAnalytics
{
    [ApiController]
    [Route("api/admin/analytics")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalyticsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("categories")]
        [ProducesResponseType(typeof(GetCategoryAnalyticsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetCategoryAnalyticsResponse>> GetCategoryAnalytics(
            [FromQuery] GetCategoryAnalyticsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
