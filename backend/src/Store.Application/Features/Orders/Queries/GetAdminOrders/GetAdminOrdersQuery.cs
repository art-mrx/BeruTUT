using MediatR;
using Store.Application.Common.Models;
using Store.Application.Features.Orders.Dtos;
using Store.Domain.Enums;

namespace Store.Application.Features.Orders.Queries.GetAdminOrders;

public class GetAdminOrdersQuery : IRequest<PaginatedList<AdminOrderDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public OrderStatus? Status { get; set; }
}
