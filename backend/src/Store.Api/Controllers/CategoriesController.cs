using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Features.Categories.Commands.CreateCategory;
using Store.Application.Features.Categories.Commands.DeleteCategory;
using Store.Application.Features.Categories.Commands.UpdateCategory;
using Store.Application.Features.Categories.Dtos;
using Store.Application.Features.Categories.Queries.GetCategories;
using Store.Application.Features.Categories.Queries.GetCategoryById;

namespace Store.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCategoriesQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCategoryByIdQuery { Id = id }, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand { Id = id }, cancellationToken);
        return NoContent();
    }
}
