using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Dashboard.GetDashboardStats
{
    [ApiController]
    [Route("api/admin/Dashboard")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(GetDashboardStatsResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetDashboardStatsResponse>> GetDashboardStats()
        {
            var result = await _mediator.Send(new GetDashboardStatsQuery());
            return Ok(result);
        }
    }
}
