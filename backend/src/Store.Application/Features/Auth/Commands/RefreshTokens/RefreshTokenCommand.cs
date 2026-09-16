using MediatR;
using Store.Application.Features.Auth.Dtos;

namespace Store.Application.Features.Auth.Commands.RefreshTokens;

public class RefreshTokenCommand : IRequest<AuthResultDto>
{
    public string RefreshToken { get; set; } = null!;
}
