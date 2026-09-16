using MediatR;

namespace Store.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    public string RefreshToken { get; set; } = null!;
}
