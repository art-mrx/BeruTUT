using MediatR;
using Store.Application.Features.Auth.Dtos;

namespace Store.Application.Features.Auth.Commands.Register;

public class RegisterCommand : IRequest<AuthResultDto>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
}
