using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Carts.Dtos;
using Store.Domain.Entities;

namespace Store.Application.Features.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, CartDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UpdateCartItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<CartDto> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var item = await _context.CartItems
            .Include(i => i.Product)
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.Cart.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(CartItem), request.ItemId);

        if (request.Quantity > item.Product.StockQuantity)
        {
            throw new ConflictException($"Only {item.Product.StockQuantity} unit(s) of \"{item.Product.Name}\" are available.");
        }

        item.Quantity = request.Quantity;
        await _context.SaveChangesAsync(cancellationToken);

        var cart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);
        return _mapper.Map<CartDto>(cart);
    }
}
