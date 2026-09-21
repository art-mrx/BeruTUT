using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Application.Common.Models;
using Store.Application.Features.Users.Commands.SetUserBlocked;
using Store.Application.Features.Users.Dtos;
using Store.Application.Features.Users.Queries.GetAdminUsers;

namespace Store.Api.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedList<AdminUserDto>>> GetAll([FromQuery] GetAdminUsersQuery query, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(query, cancellationToken));

    [HttpPost("{id:guid}/block")]
    public async Task<ActionResult<AdminUserDto>> Block(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new SetUserBlockedCommand { Id = id, IsBlocked = true }, cancellationToken));

    [HttpPost("{id:guid}/unblock")]
    public async Task<ActionResult<AdminUserDto>> Unblock(Guid id, CancellationToken cancellationToken)
        => Ok(await _mediator.Send(new SetUserBlockedCommand { Id = id, IsBlocked = false }, cancellationToken));
}
