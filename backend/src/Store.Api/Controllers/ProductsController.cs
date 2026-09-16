using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Models;
using Store.Application.Features.Products.Commands.CreateProduct;
using Store.Application.Features.Products.Commands.DeleteProduct;
using Store.Application.Features.Products.Commands.UpdateProduct;
using Store.Application.Features.Products.Commands.UploadProductImage;
using Store.Application.Features.Products.Dtos;
using Store.Application.Features.Products.Queries.GetProductById;
using Store.Application.Features.Products.Queries.GetProducts;

namespace Store.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ProductDto>>> GetAll([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{idOrSlug}")]
    public async Task<ActionResult<ProductDto>> GetById(string idOrSlug, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetProductByIdQuery { IdOrSlug = idOrSlug }, cancellationToken));

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { idOrSlug = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteProductCommand { Id = id }, cancellationToken);
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(10_000_000)]
    public async Task<ActionResult<ProductImageDto>> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();

        var command = new UploadProductImageCommand
        {
            ProductId = id,
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = stream
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
