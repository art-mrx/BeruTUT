using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Models;
using Store.Application.Features.Products.Dtos;
using Store.Application.Features.Products.Queries.GetProducts;

namespace Store.Api.Controllers;

/// <summary>
/// Admin view of the product catalog — unlike GET /api/products, this includes deactivated
/// products so admins can find and reactivate them. CRUD itself still lives on ProductsController.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<ProductDto>>> GetAll([FromQuery] GetProductsQuery query, CancellationToken cancellationToken)
    {
        query.IncludeInactive = true;
        return Ok(await _mediator.Send(query, cancellationToken));
    }
}
