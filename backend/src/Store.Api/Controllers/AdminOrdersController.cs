using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Commands.UpdateOrderStatus;
using Store.Application.Features.Orders.Dtos;
using Store.Application.Features.Orders.Queries.GetAdminOrders;

namespace Store.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<AdminOrderDto>>> GetAll([FromQuery] GetAdminOrdersQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<AdminOrderDto>> UpdateStatus(Guid id, UpdateOrderStatusCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
