using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Users.Dtos;
using Store.Domain.Entities;
using Store.Domain.Enums;

namespace Store.Application.Features.Users.Commands.SetUserBlocked;

public class SetUserBlockedCommandHandler : IRequestHandler<SetUserBlockedCommand, AdminUserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SetUserBlockedCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AdminUserDto> Handle(SetUserBlockedCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (request.IsBlocked)
        {
            // Also covers an admin blocking themselves — it would lock them out of the admin panel.
            if (user.Role == UserRole.Admin)
            {
                throw new ConflictException("Administrators cannot be blocked.");
            }

            if (!user.IsBlocked)
            {
                user.IsBlocked = true;
                user.BlockedAt = DateTime.UtcNow;

                // End every existing session: without a refresh token the user cannot renew an access token.
                var activeTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
                    .ToListAsync(cancellationToken);
                foreach (var token in activeTokens)
                {
                    token.RevokedAt = DateTime.UtcNow;
                }
            }
        }
        else
        {
            user.IsBlocked = false;
            user.BlockedAt = null;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Projected from the DB (not mapped from the tracked entity) so OrdersCount is correct.
        return await _context.Users
            .Where(u => u.Id == user.Id)
            .ProjectTo<AdminUserDto>(_mapper.ConfigurationProvider)
            .FirstAsync(cancellationToken);
    }
}
