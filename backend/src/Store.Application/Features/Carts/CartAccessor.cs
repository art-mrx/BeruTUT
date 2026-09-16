using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Interfaces;
using Store.Domain.Entities;

namespace Store.Application.Features.Carts;

internal static class CartAccessor
{
    public static async Task<Cart> GetOrCreateCartAsync(IApplicationDbContext context, Guid userId, CancellationToken cancellationToken)
    {
        var cart = await context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is not null)
        {
            return cart;
        }

        cart = new Cart { UserId = userId };
        context.Carts.Add(cart);
        await context.SaveChangesAsync(cancellationToken);
        return cart;
    }
}
