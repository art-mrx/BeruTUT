using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Domain.Entities;

namespace Store.Application.Features.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveCartItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var item = await _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.Cart.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(CartItem), request.ItemId);

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
