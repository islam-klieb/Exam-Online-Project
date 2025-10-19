using Exam_Online_API.Application.Features.Admin.Categories.CreateCategory;
using Exam_Online_API.Application.Features.Admin.Categories.DeleteCategory;
using Exam_Online_API.Application.Features.Admin.Categories.GetCategories;
using Exam_Online_API.Application.Features.Admin.Categories.GetCategoryDeletionImpact;
using Exam_Online_API.Application.Features.Admin.Categories.UpdateCategory;
using Exam_Online_API.Controllers;
using Exam_Online_API.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Exam_Online_API.Application.Features.Admin.Categories
{
    [Route("api/admin/categories")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoriesController : ApiBaseController
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
            
        }

        #region GET - List Categories
        [HttpGet]
        [ProducesResponseType(typeof(GetCategoriesResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<GetCategoriesResponse>> GetCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] CategorySortBy sortBy = CategorySortBy.Name,
            [FromQuery] SortDirection sortDirection = SortDirection.Ascending)
        {
            var query = new GetCategoriesQuery
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

        #region POST - Create Category
        [HttpPost]
        [ProducesResponseType(typeof(CreateCategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CreateCategoryResponse>> CreateCategory([FromForm] CreateCategoryCommand command)
        {
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        #endregion

        #region PUT - Update Category
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UpdateCategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateCategoryResponse>> UpdateCategory(Guid id, [FromForm] UpdateCategoryCommand command)
        {
            command.Id = id;
            var response = await _mediator.Send(command);
            return Ok(response);
        }
        #endregion

        #region DELETE - Delete Category
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(DeleteCategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteCategoryResponse>> DeleteCategory(Guid id)
        {
            var response = await _mediator.Send(new DeleteCategoryCommand { Id = id });
            return Ok(response);
        }
        #endregion

        #region Delete Impact
        [HttpGet("deletion-impact/{id:guid}")]
        [ProducesResponseType(typeof(CategoryDeletionImpact), StatusCodes.Status200OK)]
        public async Task<ActionResult<CategoryDeletionImpact>> GetCategoryDeletionImpact(Guid id)
        {
            var query = new GetCategoryDeletionImpactQuery { Id = id };
            var response = await _mediator.Send(query);
            return Ok(response);
        }
        #endregion

    }
}
