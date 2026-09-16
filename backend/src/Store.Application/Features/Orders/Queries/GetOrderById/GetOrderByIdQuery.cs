using MediatR;
using Store.Application.Features.Orders.Dtos;

namespace Store.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQuery : IRequest<OrderDto>
{
    public Guid Id { get; set; }
}
