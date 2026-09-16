using AutoMapper;
using MediatR;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Carts;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Entities;
using Store.Domain.Enums;

namespace Store.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CreateOrderCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService, IMapper mapper)
    {
        _context = context;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new AuthenticationException("No authenticated user.");

        var cart = await CartAccessor.GetOrCreateCartAsync(_context, userId, cancellationToken);

        if (cart.Items.Count == 0)
        {
            throw new ConflictException("Cannot place an order from an empty cart.");
        }

        foreach (var cartItem in cart.Items)
        {
            if (!cartItem.Product.IsActive)
            {
                throw new ConflictException($"\"{cartItem.Product.Name}\" is no longer available.");
            }

            if (cartItem.Quantity > cartItem.Product.StockQuantity)
            {
                throw new ConflictException(
                    $"Only {cartItem.Product.StockQuantity} unit(s) of \"{cartItem.Product.Name}\" are available.");
            }
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.New,
            ShippingAddress = request.ShippingAddress,
            TotalAmount = cart.Items.Sum(i => i.Product.Price * i.Quantity)
        };

        foreach (var cartItem in cart.Items)
        {
            order.Items.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                Price = cartItem.Product.Price,
                Quantity = cartItem.Quantity
            });

            cartItem.Product.StockQuantity -= cartItem.Quantity;
        }

        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(cart.Items);

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrderDto>(order);
    }
}
