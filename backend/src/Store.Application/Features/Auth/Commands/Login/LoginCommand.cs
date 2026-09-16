using MediatR;
using Store.Application.Features.Auth.Dtos;

namespace Store.Application.Features.Auth.Commands.Login;

public class LoginCommand : IRequest<AuthResultDto>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}
