using MediatR;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;

namespace Store.Application.Features.Carts.Commands.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ClearCartCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var cart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);
        if (cart.Items.Count == 0)
        {
            return;
        }

        _context.CartItems.RemoveRange(cart.Items);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
