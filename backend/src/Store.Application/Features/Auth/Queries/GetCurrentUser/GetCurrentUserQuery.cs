using MediatR;
using Store.Application.Features.Auth.Dtos;

namespace Store.Application.Features.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQuery : IRequest<UserDto>
{
}
