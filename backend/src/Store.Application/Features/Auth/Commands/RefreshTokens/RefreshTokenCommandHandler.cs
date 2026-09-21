using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Common.Settings;
using Store.Application.Features.Auth.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Auth.Commands.RefreshTokens;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private const string InvalidTokenMessage = "Invalid refresh token.";

    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly JwtSettings _jwtSettings;
    private readonly IMapper _mapper;

    public RefreshTokenCommandHandler(
        IApplicationDbContext context,
        IJwtTokenService tokenService,
        IOptions<JwtSettings> jwtSettings,
        IMapper mapper)
    {
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
        _mapper = mapper;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var incomingHash = _tokenService.HashToken(request.RefreshToken);

        var stored = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == incomingHash, cancellationToken);

        if (stored is null)
        {
            throw new AuthenticationException(InvalidTokenMessage);
        }

        // Checked before the "reused token" branch below: blocking a user revokes their refresh tokens,
        // and that must read as "blocked", not as a token-theft signal.
        if (stored.User.IsBlocked)
        {
            throw new ForbiddenException("This account has been blocked.");
        }

        if (stored.RevokedAt is not null)
        {
            // The token was already used/revoked once but is being presented again — a strong signal
            // it (or a sibling from the same rotation chain) was stolen. Kill every active session for
            // this user rather than trusting this single request.
            var activeTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == stored.UserId && rt.RevokedAt == null)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            throw new AuthenticationException("Refresh token reuse detected. All sessions have been revoked.");
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AuthenticationException("Refresh token has expired.");
        }

        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _tokenService.HashToken(newRefreshToken);
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays);

        stored.RevokedAt = DateTime.UtcNow;
        stored.ReplacedByTokenHash = newRefreshTokenHash;

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = newRefreshTokenExpiresAt
        });

        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(stored.User);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResultDto
        {
            AccessToken = accessToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenExpiresAt,
            User = _mapper.Map<UserDto>(stored.User)
        };
    }
}
