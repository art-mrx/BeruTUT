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

    /// <summary>Orders created at or after this moment (inclusive).</summary>
    public DateTime? CreatedFrom { get; set; }

    /// <summary>Orders created before this moment (exclusive), so a client can pass "start of the next day".</summary>
    public DateTime? CreatedTo { get; set; }

    /// <summary>Matches a fragment of the order number (the order id, e.g. the 8-character prefix shown in the UI).</summary>
    public string? Search { get; set; }

    /// <summary>"newest" (default) or "oldest".</summary>
    public string? SortBy { get; set; }
}
