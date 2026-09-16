using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Features.Carts.Commands.AddCartItem;
using Store.Application.Features.Carts.Commands.ClearCart;
using Store.Application.Features.Carts.Commands.RemoveCartItem;
using Store.Application.Features.Carts.Commands.UpdateCartItem;
using Store.Application.Features.Carts.Dtos;
using Store.Application.Features.Carts.Queries.GetCart;

namespace Store.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> GetCart(CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetCartQuery(), cancellationToken));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> AddItem(AddCartItemCommand command, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(command, cancellationToken));

    [HttpPut("items/{itemId:guid}")]
    public async Task<ActionResult<CartDto>> UpdateItem(Guid itemId, UpdateCartItemCommand command, CancellationToken cancellationToken)
    {
        command.ItemId = itemId;
        return Ok(await _mediator.Send(command, cancellationToken));
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveCartItemCommand { ItemId = itemId }, cancellationToken);
        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> Clear(CancellationToken cancellationToken)
    {
        await _mediator.Send(new ClearCartCommand(), cancellationToken);
        return NoContent();
    }
}
