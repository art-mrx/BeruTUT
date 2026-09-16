using MediatR;
using Store.Application.Features.Orders.Dtos;

namespace Store.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommand : IRequest<OrderDto>
{
    public string ShippingAddress { get; set; } = null!;
}
