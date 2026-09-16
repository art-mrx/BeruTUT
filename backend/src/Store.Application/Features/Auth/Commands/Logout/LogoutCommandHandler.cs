using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Interfaces;

namespace Store.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly ICurrentUserService _currentUserService;

    public LogoutCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService tokenService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _tokenService = tokenService;
        _currentUserService = currentUserService;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenService.HashToken(request.RefreshToken);

        var stored = await _context.RefreshTokens
            .FirstOrDefaultAsync(
                rt => rt.TokenHash == tokenHash && rt.UserId == _currentUserService.UserId && rt.RevokedAt == null,
                cancellationToken);

        if (stored is null)
        {
            return;
        }

        stored.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
