using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Commands.CreateOrder;
using Store.Application.Features.Orders.Dtos;
using Store.Application.Features.Orders.Queries.GetOrderById;
using Store.Application.Features.Orders.Queries.GetOrders;

namespace Store.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> Create(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<OrderDto>>> GetAll([FromQuery] GetOrdersQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new GetOrderByIdQuery { Id = id }, cancellationToken));
}
