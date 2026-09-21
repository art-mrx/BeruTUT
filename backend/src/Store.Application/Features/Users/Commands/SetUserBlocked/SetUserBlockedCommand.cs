using MediatR;
using Store.Application.Features.Users.Dtos;

namespace Store.Application.Features.Users.Commands.SetUserBlocked;

public class SetUserBlockedCommand : IRequest<AdminUserDto>
{
    public Guid Id { get; set; }
    public bool IsBlocked { get; set; }
}
