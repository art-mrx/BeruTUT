using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Carts.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Carts.Commands.AddCartItem;

public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public AddCartItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.ProductId);

        var cart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        var desiredQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (desiredQuantity > product.StockQuantity)
        {
            throw new ConflictException($"Only {product.StockQuantity} unit(s) of \"{product.Name}\" are available.");
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = desiredQuantity;
        }
        else
        {
            _context.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var updatedCart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);
        return _mapper.Map<CartDto>(updatedCart);
    }
}
