using Exam_Online_API.Application.Features.Admin.Exams.CreateExam;
using Exam_Online_API.Application.Features.Admin.Exams.DeleteExam;
using Exam_Online_API.Application.Features.Admin.Exams.GetExamDeletionImpact;
using Exam_Online_API.Application.Features.Admin.Exams.GetExams;
using Exam_Online_API.Application.Features.Admin.Exams.ToggleExamStatus;
using Exam_Online_API.Application.Features.Admin.Exams.UpdateExam;
using Exam_Online_API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Exams
{
    [ApiController]
    [Route("api/admin/exams")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ExamsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExamsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region GET - List Exams

        [HttpGet]
        [ProducesResponseType(typeof(GetExamsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetExamsResponse>> GetExamsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] DateTime? endDateFrom = null,
            [FromQuery] DateTime? endDateTo = null,
            [FromQuery] int? durationMin = null,
            [FromQuery] int? durationMax = null,
            [FromQuery] ExamSortBy sortBy = ExamSortBy.Title,
            [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
        {
            var query = new GetExamsQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                EndDateFrom = endDateFrom,
                EndDateTo = endDateTo,
                DurationMin = durationMin,
                DurationMax = durationMax,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

        #region POST - Create Exam
        [HttpPost]
        [ProducesResponseType(typeof(CreateExamResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateExamResponse>> CreateExam([FromForm] CreateExamCommand command)
        {
            var response = await _mediator.Send(command);

            return Ok(response);
        }
        #endregion

        #region PUT - Update Exam
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UpdateExamResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateExamResponse>> UpdateExam(Guid id, [FromForm] UpdateExamCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);

            return Ok(response);
        }
        #endregion

        #region DELETE - Delete Exam
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(DeleteExamResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteExamResponse>> DeleteExam(Guid id)
        {
            var response = await _mediator.Send(new DeleteExamCommand { Id = id });

            return Ok(response);
        }
        #endregion

        #region Toggle Exam
        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(typeof(ToggleExamStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ToggleExamStatusResponse>> ToggleExamStatus(Guid id)
        {
            var command = new ToggleExamStatusCommand { Id = id };
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        #endregion

        #region Delete Impact
        [HttpGet("deletion-impact/{id:guid}")]
        [ProducesResponseType(typeof(ExamDeletionImpact), StatusCodes.Status200OK)]
        public async Task<ActionResult<ExamDeletionImpact>> GetExamDeletionImpact(Guid id)
        {
            var query = new GetExamDeletionImpactQuery { Id = id };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

    }
}
