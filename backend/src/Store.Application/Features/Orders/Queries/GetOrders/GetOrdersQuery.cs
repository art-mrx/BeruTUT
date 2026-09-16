using MediatR;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Dtos;

namespace Store.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQuery : IRequest<PaginatedList<OrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
