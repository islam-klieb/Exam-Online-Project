using Exam_Online_API.Application.Features.Profile.GetProfile;
using Exam_Online_API.Application.Features.Profile.UpdateProfile;
using Exam_Online_API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Profile
{
    [Route("api/users/profile")]
    [Authorize]
    public class UsersController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GetProfileResponse>> GetProfile()
        {
            var result = await _mediator.Send(new GetProfileQuery());
            return Ok(result);
        }
        [HttpPut]
        [ProducesResponseType(typeof(UpdateProfileResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<UpdateProfileResponse>> UpdateProfile([FromForm] UpdateProfileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
