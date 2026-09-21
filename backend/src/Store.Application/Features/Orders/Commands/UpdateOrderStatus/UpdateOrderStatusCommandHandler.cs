using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Store.Application.Common.Exceptions;
using Store.Application.Common.Interfaces;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Entities;
using Store.Domain.Enums;

namespace Store.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, AdminOrderDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateOrderStatusCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AdminOrderDto> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), request.Id);

        // Also guards stock: since Cancelled is terminal, stock can only be returned once.
        if (!OrderStatusRules.CanTransition(order.Status, request.Status))
        {
            throw new ConflictException($"Cannot change order status from {order.Status} to {request.Status}.");
        }

        if (request.Status == OrderStatus.Cancelled)
        {
            // Stock was reserved (decremented) when the order was placed; rejecting it gives it back.
            foreach (var item in order.Items)
            {
                item.Product.StockQuantity += item.Quantity;
            }
        }

        order.Status = request.Status;
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<AdminOrderDto>(order);
    }
}
