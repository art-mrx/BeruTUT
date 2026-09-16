using MediatR;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Enums;

namespace Store.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommand : IRequest<AdminOrderDto>
{
    public Guid Id { get; set; }
    public OrderStatus Status { get; set; }
}
