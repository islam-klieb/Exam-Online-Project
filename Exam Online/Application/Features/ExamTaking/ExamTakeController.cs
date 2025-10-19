using Exam_Online_API.Application.Features.ExamTaking.ResumeExam;
using Exam_Online_API.Application.Features.ExamTaking.SaveProgress;
using Exam_Online_API.Application.Features.ExamTaking.StartExam;
using Exam_Online_API.Application.Features.ExamTaking.SubmitExam;
using Exam_Online_API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.ExamTaking
{
    [Route("api/exam-take")]
    [Authorize] 
    public class ExamTakeController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public ExamTakeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("start/{examId:guid}")]
        [ProducesResponseType(typeof(StartExamResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<StartExamResponse>> StartExam(Guid examId)
        {
            var command = new StartExamCommand { ExamId = examId };
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("submit")]
        [ProducesResponseType(typeof(SubmitExamResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<SubmitExamResponse>> SubmitExam(
            [FromBody] SubmitExamCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPost("save-progress")]
        [ProducesResponseType(typeof(SaveProgressResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<SaveProgressResponse>> SaveProgress(
            [FromBody] SaveProgressCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpGet("resume/{examId:guid}")]
        [ProducesResponseType(typeof(ResumeExamResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ResumeExamResponse>> CheckResume(Guid examId)
        {
            var query = new ResumeExamQuery { ExamId = examId };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}
